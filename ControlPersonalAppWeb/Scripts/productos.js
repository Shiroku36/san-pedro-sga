
var items = 0;
var eliminar = document.getElementById("eliminar");
var stocks = @Html.Raw(Json.Encode(ViewBag.productos));
function agregarEliminar() {
    //Si hay más de 1 item agrega el noton eliminar
    if (items > 0) {
        eliminar.style.display = "inline-block";
    }
}
//Carga los huertos cuando se carga la página o cuando se agrega uno nuevo
//le agrega la cantidad máxima a cada uno
function cargarHuertos(inicio) {
    //Obtiene todos los select de clase producto
    var productos = document.getElementsByClassName('producto');
    var i, j;
    var nombres = [];
    //Para cada select encontrado
    for (j = inicio; j < productos.length; j++) {
        var producto = productos[j];
        //Se sacan todos los productos que ya tenía por si cambia de huerto
        for (i = producto.length; i >= 0; i--) {
            producto.remove(i);
        }
        //El origen es de donde hay que poner los productos
        var origen = document.getElementById("Origen");
        //Para cada producto en la lista que viene
        for (var i in stocks) {
            var stock = stocks[i];
            //Si coincide con su ubicacion se agrega al select actual
            if (stock.Ubicacion == origen.options[origen.selectedIndex].text) {
                producto.add(new Option(stock.Producto));
            }
        }
        //Siempre quedará el primer producto como primero, así que busca cual es y le pone su cantidad
        i = 0;
        while (producto.options[0].text != stocks[i].Producto || origen.value != stocks[i].Ubicacion) {
            i++;
        }
        document.getElementById("Cantidad " + j).value = stocks[i].Cantidad;
    }
}
//Busca la cantidad máxima para cada producto
function maximo(producto) {
    //No siempre funciona el this, así que se pasa por parametro algunas veces, como sea siempre habrá 1 que será válido
    var producto = document.getElementById(producto.id) || document.getElementById(this.id);
    var origen = document.getElementById("Origen");
    //Divide el nombre para obtener el número del cual se seleccionó
    var id = String(producto.id).split(' ');
    i = 0;
    //Busca la ubicación y el producto para obtener la posición y luego asignarle la cantidad de ese producto, en el producto seleccionado
    while (stocks[i].Ubicacion != origen.value || stocks[i].Producto != producto.value) {
        i++;
    }
    document.getElementById("Cantidad " + id[1]).value = stocks[i].Cantidad;
}
//Comprueba que no se pase de la cantidad que el producto posee
function maximoAceptable(cantidad) {
    //Recibe la entrada
    var cantidad = document.getElementById(cantidad.id) || document.getElementById(this.id);
    var origen = document.getElementById("Origen");
    var id = String(cantidad.id).split(' ');
    var producto = document.getElementById("Producto " + id[1]);
    i = 0;
    while (stocks[i].Ubicacion != origen.value || stocks[i].Producto != producto.value) {
        i++;
    }
    //Comprueba si es mayor y la reemplaza a la máxima, además da una alerta del máximo
    if (cantidad.value > stocks[i].Cantidad) {
        cantidad.value = stocks[i].Cantidad;
        alert("Cantidad máxima: " + stocks[i].Cantidad);
    }

}
//Elimina un campo de producto y cantidad
function eliminarProducto() {
    //Selecciona todos los productos y cantidades agregados fuera del primero, todos los demás tienen
    //una clase llamado extra
    var selects = document.getElementsByClassName('extra');
    selects[items].remove();
    selects[items - 1].remove();
    items = items - 1;
    //Comprueba si hay más productos agregados, si queda solo 1, oculta el boton eliminar
    if (items == 0) {
        eliminar.style.display = "none";
    }
}

function agregarProducto() {
    items = items + 1;
    var contenedor = document.getElementById("contenedor");
    var divExternoProducto = document.createElement("div");
    divExternoProducto.className = "form-group extra";
    var labelProducto = document.createElement("label");
    var textoProducto = document.createTextNode("Producto");
    labelProducto.appendChild(textoProducto);
    labelProducto.className = "control-label col-md-2";
    var divInternoProducto = document.createElement("div");
    divInternoProducto.className = "col-md-10";
    divExternoProducto.appendChild(labelProducto);
    var select = document.createElement("select");
    select.id = "Producto " + items;
    select.name = "Producto";
    select.className = "form-control producto ubicacion";
    select.required = "required";
    select.onchange = maximo;
    divInternoProducto.appendChild(select);
    divExternoProducto.appendChild(divInternoProducto);
    contenedor.appendChild(divExternoProducto);

    var divExternoCantidad = document.createElement("div");
    divExternoCantidad.className = "form-group extra";
    var labelCantidad = document.createElement("label");
    var textoCantidad = document.createTextNode("Cantidad");
    labelCantidad.appendChild(textoCantidad);
    labelCantidad.className = "control-label col-md-2";
    var divInternoCantidad = document.createElement("div");
    divInternoCantidad.className = "col-md-10";
    divExternoCantidad.appendChild(labelCantidad);
    var cantidad = document.createElement("input");
    cantidad.id = "Cantidad " + items;
    cantidad.name = "Cantidad";
    cantidad.className = "form-control ubicacion";
    cantidad.required = "required";
    cantidad.type = "numero";
    cantidad.oninput = maximoAceptable;
    divInternoCantidad.appendChild(cantidad);
    divExternoCantidad.appendChild(divInternoCantidad);
    contenedor.appendChild(divExternoCantidad);
    cargarHuertos(items);
    agregarEliminar();
}

window.onload = cargarHuertos(0);