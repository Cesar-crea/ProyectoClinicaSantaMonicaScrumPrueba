function abrirHorarios() {

    ```
const medicoInput =
    document.getElementById("medicoId");

const fechaInput =
    document.getElementById("fechaCita");


// =====================================================
// VALIDAR MÉDICO
// =====================================================

if (!medicoInput) {

    console.error(
        "No existe el elemento #medicoId"
    );

    Swal.fire(
        "Error",
        "No se encontró el médico.",
        "error"
    );

    return;
}


// =====================================================
// VALIDAR FECHA
// =====================================================

if (!fechaInput) {

    console.error(
        "No existe el elemento #fechaCita"
    );

    Swal.fire(
        "Error",
        "No se encontró la fecha.",
        "error"
    );

    return;
}


const idMedico =
    medicoInput.value;

const fecha =
    fechaInput.value;


mostrarHorarios(
    idMedico,
    fecha
);
```

}

// =========================================================
// MOSTRAR HORARIOS
// =========================================================

function mostrarHorarios(
    idMedico,
    fecha
) {

    ```
// -----------------------------------------------------
// Validar médico
// -----------------------------------------------------

if (!idMedico) {

    Swal.fire(
        "Médico requerido",
        "Seleccione un médico primero.",
        "warning"
    );

    return;
}


// -----------------------------------------------------
// Validar fecha
// -----------------------------------------------------

if (!fecha) {

    Swal.fire(
        "Fecha requerida",
        "Seleccione una fecha.",
        "warning"
    );

    return;
}


// -----------------------------------------------------
// Cargar horarios desde CitaController
// -----------------------------------------------------

const url =
    `/ Cita / HorariosOcupados ? idMedico = ${ encodeURIComponent(idMedico) }& fecha=${ encodeURIComponent(fecha) } `;


fetch(url)

    .then(response => {

        if (!response.ok) {

            throw new Error(
                `Error HTTP ${ response.status } `
            );

        }

        return response.text();

    })

    .then(html => {

        const container =
            document.getElementById(
                "modalHorariosContainer"
            );


        if (!container) {

            throw new Error(
                "No existe #modalHorariosContainer"
            );

        }


        // -------------------------------------------------
        // Insertar contenido del modal
        // -------------------------------------------------

        container.innerHTML =
            html;


        // -------------------------------------------------
        // Obtener modal
        // -------------------------------------------------

        const modalElement =
            document.getElementById(
                "modalHorarios"
            );


        if (!modalElement) {

            throw new Error(
                "No existe #modalHorarios"
            );

        }


        // -------------------------------------------------
        // Mostrar modal
        // -------------------------------------------------

        const modal =
            bootstrap.Modal.getOrCreateInstance(
                modalElement
            );

        modal.show();

    })

    .catch(error => {

        console.error(
            "Error al cargar horarios:",
            error
        );


        Swal.fire(
            "Error",
            "No se pudieron cargar los horarios.",
            "error"
        );

    });
```

}

// =========================================================
// CERRAR / LIMPIAR MODAL DE HORARIOS
// =========================================================

function cerrarModal() {

    ```
const container =
    document.getElementById(
        "modalHorariosContainer"
    );


if (container) {

    container.innerHTML = "";

}
```

}
