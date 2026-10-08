// ==========================================================
// CITAS - HORARIOS
// ==========================================================


function abrirHorarios() {

    const medicoInput =
        document.getElementById("medicoId");

    const fechaInput =
        document.getElementById("fechaCita");


    // ======================================================
    // VALIDAR MEDICO
    // ======================================================

    if (!medicoInput ||
        !medicoInput.value) {

        Swal.fire(
            "Médico requerido",
            "Seleccione un médico primero.",
            "warning"
        );

        return;
    }


    // ======================================================
    // VALIDAR FECHA
    // ======================================================

    if (!fechaInput ||
        !fechaInput.value) {

        Swal.fire(
            "Fecha requerida",
            "Seleccione una fecha primero.",
            "warning"
        );

        return;
    }


    // ======================================================
    // FECHA LOCAL
    // ======================================================

    const ahora =
        new Date();


    const fechaHoy =
        `${ahora.getFullYear()}-${String(
            ahora.getMonth() + 1
        ).padStart(2, "0")}-${String(
            ahora.getDate()
        ).padStart(2, "0")}`;


    // ======================================================
    // VALIDAR FECHA ANTERIOR
    // ======================================================

    if (fechaInput.value < fechaHoy) {

        Swal.fire(
            "Fecha no válida",
            "No puede consultar horarios de fechas anteriores a hoy.",
            "warning"
        );

        return;
    }


    // ======================================================
    // CARGAR MODAL
    // ======================================================

    mostrarHorarios(
        medicoInput.value,
        fechaInput.value
    );

}


// ==========================================================
// MOSTRAR HORARIOS
// ==========================================================

async function mostrarHorarios(
    idMedico,
    fecha
) {

    try {

        // ==================================================
        // URL
        // ==================================================

        const url =
            `/Cita/HorariosOcupados?idMedico=${encodeURIComponent(
                idMedico
            )}&fecha=${encodeURIComponent(
                fecha
            )}`;


        // ==================================================
        // REQUEST
        // ==================================================

        const response =
            await fetch(url);


        if (!response.ok) {

            throw new Error(
                `Error HTTP ${response.status}`
            );

        }


        // ==================================================
        // HTML DEL PARTIAL
        // ==================================================

        const html =
            await response.text();


        // ==================================================
        // CONTENEDOR
        // ==================================================

        const container =
            document.getElementById(
                "modalHorariosContainer"
            );


        if (!container) {

            throw new Error(
                "No existe #modalHorariosContainer."
            );

        }


        container.innerHTML =
            html;


        // ==================================================
        // MODAL
        // ==================================================

        const modalElement =
            document.getElementById(
                "modalHorarios"
            );


        if (!modalElement) {

            throw new Error(
                "No existe #modalHorarios."
            );

        }


        // ==================================================
        // BOOTSTRAP MODAL
        // ==================================================

        const modal =
            bootstrap.Modal.getOrCreateInstance(
                modalElement
            );


        modal.show();

    }
    catch (error) {

        console.error(
            "Error al cargar horarios:",
            error
        );


        Swal.fire(
            "Error",
            "No se pudieron cargar los horarios.",
            "error"
        );

    }

}


// ==========================================================
// CERRAR MODAL
// ==========================================================

function cerrarModal() {

    const container =
        document.getElementById(
            "modalHorariosContainer"
        );


    if (container) {

        container.innerHTML =
            "";

    }

}