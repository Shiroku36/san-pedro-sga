function buscar() {
    var input, filter, table, i, j;
    input = document.getElementById("busqueda");
    filter = input.value.toUpperCase();
    table = document.getElementById("tabla");
    trs = table.getElementsByTagName("tr");
    for (i = 1; i < trs.length; i++) {
        var esta = false;
        for (j = 0; j < trs[i].getElementsByTagName("td").length && !esta; j++){
            var valor = trs[i].getElementsByTagName("td")[j].textContent || trs[i].getElementsByTagName("td")[j].innerText;
            if (valor.toUpperCase().indexOf(filter) > -1) {
                esta = true;
            }
        }
        if (esta) {
            trs[i].style.display = "";
        } else {
            trs[i].style.display = "none";
        }
    }

}