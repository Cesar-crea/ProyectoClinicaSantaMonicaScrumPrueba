
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ProyectoSantaMonica_Cesar.Data;
using ProyectoSantaMonica_Cesar.Models.ViewModels;
using ProyectoSantaMonica_Cesar.Models.enums;

namespace ProyectoSantaMonica_Cesar.Service.ReporteService
{
    public class ReporteService : IReporteService
    {
        private readonly ApplicationDbContext _context;

        public ReporteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReporteCitaRow>> ObtenerCitasAsync(
            ReporteFiltrosViewModel filtros)
        {
            if (filtros.Desde.Date > filtros.Hasta.Date)
            {
                throw new ArgumentException(
                    "La fecha inicial no puede ser mayor que la fecha final.");
            }

            var query = _context.Cita
                .AsNoTracking()
                .Include(x => x.Paciente)
                .Include(x => x.Medico)
                .ThenInclude(m => m!.especialidad)
                .AsQueryable();

            // FILTRO POR FECHAS
            query = query.Where(x =>
                x.Fecha.Date >= filtros.Desde.Date &&
                x.Fecha.Date <= filtros.Hasta.Date);

            // FILTRO POR MÉDICO
            if (filtros.Id_Medico.HasValue)
            {
                query = query.Where(x =>
                    x.Id_Medico == filtros.Id_Medico.Value);
            }

            // FILTRO POR ESTADO
            if (!string.IsNullOrWhiteSpace(filtros.Estado))
            {
                if (!Enum.TryParse<EstadoCita>(
                    filtros.Estado,
                    true,
                    out var estado))
                {
                    throw new ArgumentException(
                        "Estado de cita inválido.");
                }

                query = query.Where(x => x.Estado == estado);
            }

            // FILTRO POR ESPECIALIDAD
            if (!string.IsNullOrWhiteSpace(filtros.Especialidad))
            {
                query = query.Where(x =>
                    x.Medico!.especialidad!.Nombre ==
                    filtros.Especialidad);
            }

            // BÚSQUEDA GENERAL
            if (!string.IsNullOrWhiteSpace(filtros.Buscar))
            {
                var buscar = filtros.Buscar.Trim();

                query = query.Where(x =>
                    (
                        (x.Paciente!.Nombres ?? "") + " " +
                        (x.Paciente.Apellidos ?? "")
                    ).Contains(buscar)

                    || (x.Paciente.Dni ?? "").Contains(buscar)

                    || (
                        (x.Medico!.Nombres ?? "") + " " +
                        (x.Medico.Apellidos ?? "")
                    ).Contains(buscar)

                    || (
                        x.Medico!.especialidad != null
                        && x.Medico.especialidad.Nombre.Contains(buscar)
                    )

                    || (x.Motivo ?? "").Contains(buscar)

                    || x.Estado.ToString().Contains(buscar)
                );
            }

            // RESULTADOS ORDENADOS
            return await query

.OrderBy(x => x.Estado.ToString() == "PENDIENTE" ? 0 : 1)
.ThenBy(x => x.Fecha)
.ThenBy(x => x.Hora)
                .Select(x => new ReporteCitaRow
                {
                    Id_Cita = x.Id_Cita,

                    Paciente = (
                        (x.Paciente!.Nombres ?? "") + " " +
                        (x.Paciente.Apellidos ?? "")
                    ).Trim(),

                    Dni = x.Paciente!.Dni,

                    Medico = (
                        (x.Medico!.Nombres ?? "") + " " +
                        (x.Medico.Apellidos ?? "")
                    ).Trim(),

                    Especialidad =
                        x.Medico!.especialidad != null
                            ? x.Medico.especialidad.Nombre
                            : string.Empty,

                    Fecha = x.Fecha,

                    Hora = x.Hora,

                    Estado = x.Estado.ToString(),

                    Motivo = x.Motivo ?? string.Empty
                })
                .ToListAsync();
        }

        public Task<List<ReporteCmpRow>> ObtenerCmpAsync()
        {
            return _context.Medico
                .AsNoTracking()
                .Include(x => x.especialidad)
                .OrderBy(x => x.Apellidos)
                .ThenBy(x => x.Nombres)
                .Select(x => new ReporteCmpRow
                {
                    Id_Medico = x.Id_Medico,

                    Medico = (
                        (x.Nombres ?? "") + " " +
                        (x.Apellidos ?? "")
                    ).Trim(),

                    Dni = x.Dni,

                    CMP = x.Nro_Colegiatura,

                    Especialidad = x.especialidad != null
                        ? x.especialidad.Nombre
                        : string.Empty,

                    Telefono = x.Telefono
                })
                .ToListAsync();
        }

        public byte[] GenerarCitasExcel(
            IReadOnlyCollection<ReporteCitaRow> rows,
            ReporteFiltrosViewModel filtros)
        {
            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Reporte Citas");

            worksheet.Cell(1, 1).Value =
                "CLÍNICA SANTA MÓNICA - REPORTE DE CITAS";

            worksheet.Range(1, 1, 1, 9).Merge();
            worksheet.Range(1, 1, 1, 9).Style.Font.SetBold();
            worksheet.Range(1, 1, 1, 9).Style.Font.FontSize = 15;

            worksheet.Cell(2, 1).Value =
                $"Periodo: {filtros.Desde:dd/MM/yyyy} - {filtros.Hasta:dd/MM/yyyy}";

            worksheet.Range(2, 1, 2, 9).Merge();

            string[] headers =
            {
                "Cod. Cita", "Paciente", "DNI", "Médico",
                "Especialidad", "Fecha", "Hora", "Estado", "Motivo"
            };

            for (var i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(4, i + 1).Value = headers[i];
            }

            var rowNumber = 5;

            foreach (var row in rows)
            {
                worksheet.Cell(rowNumber, 1).Value = row.Id_Cita;
                worksheet.Cell(rowNumber, 2).Value = row.Paciente;
                worksheet.Cell(rowNumber, 3).Value = row.Dni;
                worksheet.Cell(rowNumber, 4).Value = row.Medico;
                worksheet.Cell(rowNumber, 5).Value = row.Especialidad;
                worksheet.Cell(rowNumber, 6).Value =
                    row.Fecha.ToString("dd/MM/yyyy");
                worksheet.Cell(rowNumber, 7).Value =
                    row.Hora.ToString(@"hh\:mm");
                worksheet.Cell(rowNumber, 8).Value = row.Estado;
                worksheet.Cell(rowNumber, 9).Value = row.Motivo;

                rowNumber++;
            }

            var headerRange = worksheet.Range(4, 1, 4, 9);

            headerRange.Style.Font.SetBold();
            headerRange.Style.Fill.SetBackgroundColor(
                XLColor.FromHtml("#203354"));
            headerRange.Style.Font.SetFontColor(XLColor.White);

            worksheet.Columns().AdjustToContents();
            worksheet.SheetView.FreezeRows(4);

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }

        public byte[] GenerarCmpExcel(
            IReadOnlyCollection<ReporteCmpRow> rows)
        {
            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Reporte CMP");

            worksheet.Cell(1, 1).Value =
                "CLÍNICA SANTA MÓNICA - REPORTE CMP";

            worksheet.Range(1, 1, 1, 6).Merge();
            worksheet.Range(1, 1, 1, 6).Style.Font.SetBold();
            worksheet.Range(1, 1, 1, 6).Style.Font.FontSize = 15;

            string[] headers =
            {
                "ID Médico", "Médico", "DNI",
                "CMP", "Especialidad", "Teléfono"
            };

            for (var i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(3, i + 1).Value = headers[i];
            }

            var rowNumber = 4;

            foreach (var row in rows)
            {
                worksheet.Cell(rowNumber, 1).Value = row.Id_Medico;
                worksheet.Cell(rowNumber, 2).Value = row.Medico;
                worksheet.Cell(rowNumber, 3).Value = row.Dni;
                worksheet.Cell(rowNumber, 4).Value = row.CMP;
                worksheet.Cell(rowNumber, 5).Value = row.Especialidad;
                worksheet.Cell(rowNumber, 6).Value = row.Telefono;

                rowNumber++;
            }

            var headerRange = worksheet.Range(3, 1, 3, 6);

            headerRange.Style.Font.SetBold();
            headerRange.Style.Fill.SetBackgroundColor(
                XLColor.FromHtml("#203354"));
            headerRange.Style.Font.SetFontColor(XLColor.White);

            worksheet.Columns().AdjustToContents();
            worksheet.SheetView.FreezeRows(3);

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }

        public byte[] GenerarCitasPdf(
            IReadOnlyCollection<ReporteCitaRow> rows,
            ReporteFiltrosViewModel filtros)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header()
                        .Text("CLÍNICA SANTA MÓNICA")
                        .Bold()
                        .FontSize(18)
                        .FontColor("203354");

                    page.Content().Column(column =>
                    {
                        column.Spacing(8);

                        column.Item()
                            .Text("Reporte de Citas")
                            .Bold()
                            .FontSize(15);

                        column.Item().Text(
                            $"Periodo: {filtros.Desde:dd/MM/yyyy} - {filtros.Hasta:dd/MM/yyyy}");

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(35);
                                columns.RelativeColumn(1.3f);
                                columns.ConstantColumn(65);
                                columns.RelativeColumn(1.3f);
                                columns.RelativeColumn(1.1f);
                                columns.ConstantColumn(65);
                                columns.ConstantColumn(45);
                                columns.ConstantColumn(70);
                            });

                            string[] headers =
                            {
                                "Cod. Cita", "Paciente", "DNI", "Médico",
                                "Especialidad", "Fecha", "Hora", "Estado"
                            };

                            foreach (var header in headers)
                            {
                                table.Cell()
                                    .Element(Header)
                                    .Text(header);
                            }

                            foreach (var row in rows)
                            {
                                table.Cell().Text(row.Id_Cita.ToString());
                                table.Cell().Text(row.Paciente);
                                table.Cell().Text(row.Dni);
                                table.Cell().Text(row.Medico);
                                table.Cell().Text(row.Especialidad);
                                table.Cell().Text(
                                    row.Fecha.ToString("dd/MM/yyyy"));
                                table.Cell().Text(
                                    row.Hora.ToString(@"hh\:mm"));
                                table.Cell().Text(row.Estado);
                            }
                        });

                        column.Item()
                            .Text($"Total de citas: {rows.Count}")
                            .Bold();
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}");
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerarCmpPdf(
            IReadOnlyCollection<ReporteCmpRow> rows)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header()
                        .Text("CLÍNICA SANTA MÓNICA")
                        .Bold()
                        .FontSize(18)
                        .FontColor("203354");

                    page.Content().Column(column =>
                    {
                        column.Spacing(8);

                        column.Item()
                            .Text("Reporte de Médicos - CMP")
                            .Bold()
                            .FontSize(15);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.5f);
                                columns.ConstantColumn(65);
                                columns.ConstantColumn(70);
                                columns.RelativeColumn(1.2f);
                                columns.ConstantColumn(80);
                            });

                            string[] headers =
                            {
                                "Médico", "DNI", "CMP",
                                "Especialidad", "Teléfono"
                            };

                            foreach (var header in headers)
                            {
                                table.Cell()
                                    .Element(Header)
                                    .Text(header);
                            }

                            foreach (var row in rows)
                            {
                                table.Cell().Text(row.Medico);
                                table.Cell().Text(row.Dni);
                                table.Cell().Text(row.CMP);
                                table.Cell().Text(row.Especialidad);
                                table.Cell().Text(row.Telefono);
                            }
                        });

                        column.Item()
                            .Text($"Total de médicos: {rows.Count}")
                            .Bold();
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}");
                });
            });

            return document.GeneratePdf();
        }

        private static IContainer Header(IContainer container)
        {
            return container
                .Background("203354")
                .Padding(4)
                .DefaultTextStyle(
                    style => style.FontColor(Colors.White).Bold())
                .AlignCenter();
        }

        public async Task<List<ReporteComprobantePagoRow>> ObtenerComprobantesAsync(
    ReporteComprobantePagoFiltrosViewModel filtros)
        {
            var query = _context.ComprobanteDePago
                .AsNoTracking()
                .Include(x => x.Cita)
                    .ThenInclude(c => c!.Paciente)
                .AsQueryable();

            if (filtros.Desde.HasValue)
                query = query.Where(x =>
                    x.Fecha_Emision >= filtros.Desde.Value.Date);

            if (filtros.Hasta.HasValue)
            {
                var hastaExclusivo = filtros.Hasta.Value.Date.AddDays(1);

                query = query.Where(x =>
                    x.Fecha_Emision < hastaExclusivo);
            }

            if (!string.IsNullOrWhiteSpace(filtros.Estado) &&
                Enum.TryParse<EstadoComprobante>(
                    filtros.Estado, true, out var estado))
            {
                query = query.Where(x => x.Estado == estado);
            }

            // FILTRO POR MÉTODO DE PAGO
            if (!string.IsNullOrWhiteSpace(filtros.Metodo) &&
                Enum.TryParse<MetodoPago>(
                    filtros.Metodo.Trim(),
                    true,
                    out var metodo))
            {
                var metodoTexto = metodo.ToString();

                query = query.Where(x => x.Metodo_Pago == metodoTexto);
            }

            if (!string.IsNullOrWhiteSpace(filtros.Buscar))
            {
                var buscar = filtros.Buscar.Trim();

                query = query.Where(x =>
                    x.Nombre_Pagador.Contains(buscar) ||
                    x.Apellidos_Pagador.Contains(buscar) ||
                    (x.Dni_Pagador != null &&
                     x.Dni_Pagador.Contains(buscar)) ||
                    x.Id_Comprobante.ToString().Contains(buscar));
            }

            return await query
                // Regla principal: EMITIDO siempre antes que ANULADO.
                .OrderBy(x => x.Estado == EstadoComprobante.EMITIDO ? 0 : 1)
                // Regla secundaria: el más reciente primero.
                .ThenByDescending(x => x.Fecha_Emision)
                .ThenByDescending(x => x.Id_Comprobante)
                .Select(x => new ReporteComprobantePagoRow
                {
                    Id_Comprobante = x.Id_Comprobante,
                    Nombre = x.Nombre_Pagador,
                    Apellidos = x.Apellidos_Pagador,
                    Dni = x.Dni_Pagador ?? "",
                    Fecha = x.Fecha_Emision,
                    Monto = x.Monto,
                    Metodo = x.Metodo_Pago.ToString(),
                    Estado = x.Estado.ToString()
                })
                .ToListAsync();
        }

        public byte[] GenerarComprobantesExcel(
     IReadOnlyCollection<ReporteComprobantePagoRow> rows)
        {
            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Historial de Pagos");

            // Título del reporte
            worksheet.Cell(1, 1).Value =
                "CLÍNICA SANTA MÓNICA - HISTORIAL DE COMPROBANTES DE PAGO";

            worksheet.Range(1, 1, 1, 8).Merge();
            worksheet.Range(1, 1, 1, 8).Style.Font.SetBold();
            worksheet.Range(1, 1, 1, 8).Style.Font.FontSize = 15;
            worksheet.Range(1, 1, 1, 8).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            worksheet.Cell(2, 1).Value =
                $"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}";

            worksheet.Range(2, 1, 2, 8).Merge();

            // Encabezados
            string[] headers =
            {
        "Nombre",
        "Apellidos",
        "DNI",
        "Fecha",
        "Monto",
        "Método",
        "Estado",
        "ID CP"
    };

            for (int i = 0; i < headers.Length; i++)
                worksheet.Cell(4, i + 1).Value = headers[i];

            // Datos
            int rowNumber = 5;

            foreach (var row in rows)
            {
                worksheet.Cell(rowNumber, 1).Value = row.Nombre;
                worksheet.Cell(rowNumber, 2).Value = row.Apellidos;
                worksheet.Cell(rowNumber, 3).Value = row.Dni;

                worksheet.Cell(rowNumber, 4).Value = row.Fecha;
                worksheet.Cell(rowNumber, 4).Style.DateFormat.Format =
                    "dd/MM/yyyy";

                worksheet.Cell(rowNumber, 5).Value = row.Monto;
                worksheet.Cell(rowNumber, 5).Style.NumberFormat.Format =
                    "0.00";

                worksheet.Cell(rowNumber, 6).Value = row.Metodo;
                worksheet.Cell(rowNumber, 7).Value = row.Estado;
                worksheet.Cell(rowNumber, 8).Value = row.Id_Comprobante;

                rowNumber++;
            }

            // Estilo de encabezados
            var headerRange = worksheet.Range(4, 1, 4, 8);

            headerRange.Style.Font.SetBold();
            headerRange.Style.Fill.SetBackgroundColor(
                XLColor.FromHtml("#203354"));
            headerRange.Style.Font.SetFontColor(XLColor.White);
            headerRange.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            // Bordes y presentación
            var usedRange = worksheet.Range(
                4, 1, Math.Max(4, rowNumber - 1), 8);

            usedRange.Style.Border.OutsideBorder =
                XLBorderStyleValues.Thin;

            usedRange.Style.Border.InsideBorder =
                XLBorderStyleValues.Thin;

            worksheet.Columns().AdjustToContents();
            worksheet.SheetView.FreezeRows(4);
            worksheet.RangeUsed().SetAutoFilter();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return stream.ToArray();
        }

        public byte[] GenerarComprobantesPdf(
      IReadOnlyCollection<ReporteComprobantePagoRow> rows)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Text("CLÍNICA SANTA MÓNICA")
                                .Bold()
                                .FontSize(18)
                                .FontColor("203354");

                            column.Item()
                                .Text("Historial de Comprobante de Pago")
                                .Bold()
                                .FontSize(14);

                            column.Item()
                                .Text(
                                    $"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(9);
                        });

                    page.Content()
                        .PaddingVertical(12)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.2f); // Nombre
                                columns.RelativeColumn(1.2f); // Apellidos
                                columns.RelativeColumn(1.0f); // DNI
                                columns.RelativeColumn(1.0f); // Fecha
                                columns.RelativeColumn(0.8f); // Monto
                                columns.RelativeColumn(1.2f); // Método
                                columns.RelativeColumn(1.0f); // Estado
                                columns.RelativeColumn(0.7f); // ID CP
                            });

                            string[] headers =
                            {
                        "Nombre",
                        "Apellidos",
                        "DNI",
                        "Fecha",
                        "Monto",
                        "Método",
                        "Estado",
                        "ID CP"
                            };

                            foreach (var header in headers)
                            {
                                table.Cell()
                                    .Element(Header)
                                    .Text(header)
                                    .FontSize(9);
                            }

                            foreach (var row in rows)
                            {
                                table.Cell().Padding(4)
                                    .Text(row.Nombre).FontSize(9);

                                table.Cell().Padding(4)
                                    .Text(row.Apellidos).FontSize(9);

                                table.Cell().Padding(4)
                                    .Text(row.Dni).FontSize(9);

                                table.Cell().Padding(4)
                                    .Text(row.Fecha.ToString("dd/MM/yyyy"))
                                    .FontSize(9);

                                table.Cell().Padding(4)
                                    .Text(row.Monto.ToString("0.00"))
                                    .FontSize(9);

                                table.Cell().Padding(4)
                                    .Text(row.Metodo).FontSize(9);

                                table.Cell().Padding(4)
                                    .Text(row.Estado).FontSize(9);

                                table.Cell().Padding(4)
                                    .Text(row.Id_Comprobante.ToString())
                                    .FontSize(9);
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Total de comprobantes: ");
                            text.Span(rows.Count.ToString()).Bold();
                            text.Span(
                                $"     |     Generado el {DateTime.Now:dd/MM/yyyy HH:mm}");
                        });
                });
            });

            return document.GeneratePdf();
        }


        public byte[] GenerarHistorialExcel(
            IReadOnlyCollection<ProyectoSantaMonica_Cesar.Models.HistorialUsuario> rows)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Historial Usuarios");

            ws.Cell(1, 1).Value = "CLÍNICA SANTA MÓNICA - HISTORIAL DE USUARIOS";
            ws.Range(1, 1, 1, 8).Merge();
            ws.Range(1, 1, 1, 8).Style.Font.SetBold();
            ws.Range(1, 1, 1, 8).Style.Font.FontSize = 15;
            ws.Range(1, 1, 1, 8).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            ws.Cell(2, 1).Value = $"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}";
            ws.Range(2, 1, 2, 8).Merge();

            string[] headers =
            {
        "Cod.Hist", "Fecha", "Usuario", "Rol",
        "Módulo", "Acción", "Descripción", "IP"
    };

            for (int i = 0; i < headers.Length; i++)
                ws.Cell(4, i + 1).Value = headers[i];

            int fila = 5;

            foreach (var x in rows)
            {
                ws.Cell(fila, 1).Value = x.Id_Historial;
                ws.Cell(fila, 2).Value = x.Fecha;
                ws.Cell(fila, 2).Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";
                ws.Cell(fila, 3).Value = x.Usuario?.Username ?? $"Usuario ID {x.Id_Usuario}";
                ws.Cell(fila, 4).Value = x.Usuario != null ? x.Usuario.Rol.ToString() : "-";
                ws.Cell(fila, 5).Value = x.Modulo ?? "";
                ws.Cell(fila, 6).Value = x.Accion ?? "";
                ws.Cell(fila, 7).Value = x.Descripcion ?? "-";
                ws.Cell(fila, 8).Value = x.Ip ?? "-";
                fila++;
            }

            var encabezado = ws.Range(4, 1, 4, 8);
            encabezado.Style.Font.SetBold();
            encabezado.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#203354"));
            encabezado.Style.Font.SetFontColor(XLColor.White);
            encabezado.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var rango = ws.Range(4, 1, Math.Max(4, fila - 1), 8);
            rango.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rango.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(4);
            ws.RangeUsed().SetAutoFilter();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GenerarHistorialPdf(
            IReadOnlyCollection<ProyectoSantaMonica_Cesar.Models.HistorialUsuario> rows)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);

                    page.Header().Column(column =>
                    {
                        column.Item()
                            .Text("CLÍNICA SANTA MÓNICA")
                            .Bold().FontSize(18).FontColor("203354");

                        column.Item()
                            .Text("Historial de usuarios")
                            .Bold().FontSize(14);

                        column.Item()
                            .Text($"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(9);
                    });

                    page.Content().PaddingVertical(12).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.ConstantColumn(95);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(0.9f);
                            columns.RelativeColumn(0.9f);
                            columns.RelativeColumn(1.0f);
                            columns.RelativeColumn(2.0f);
                            columns.RelativeColumn(1.0f);
                        });

                        string[] headers =
                        {
                    "Cod.Hist", "Fecha", "Usuario", "Rol",
                    "Módulo", "Acción", "Descripción", "IP"
                };

                        foreach (var header in headers)
                        {
                            table.Cell().Element(Header).Padding(3)
                                .Text(header).FontSize(8);
                        }

                        foreach (var x in rows)
                        {
                            table.Cell().Padding(3).Text(x.Id_Historial.ToString()).FontSize(7);
                            table.Cell().Padding(3).Text(x.Fecha.ToString("dd/MM/yyyy HH:mm")).FontSize(7);
                            table.Cell().Padding(3).Text(x.Usuario?.Username ?? $"Usuario ID {x.Id_Usuario}").FontSize(7);
                            table.Cell().Padding(3).Text(x.Usuario != null ? x.Usuario.Rol.ToString() : "-").FontSize(7);
                            table.Cell().Padding(3).Text(x.Modulo ?? "").FontSize(7);
                            table.Cell().Padding(3).Text(x.Accion ?? "").FontSize(7);
                            table.Cell().Padding(3).Text(x.Descripcion ?? "-").FontSize(7);
                            table.Cell().Padding(3).Text(x.Ip ?? "-").FontSize(7);
                        }
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span($"Total de registros: {rows.Count}   |   ");
                        text.Span($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}");
                    });
                });
            });

            return document.GeneratePdf();
        }


    }
}

