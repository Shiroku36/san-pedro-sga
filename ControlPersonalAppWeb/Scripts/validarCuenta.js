
var rut = "";
rut = document.getElementById("Rut").getAttribute("value");
var ruty = rut.substring(0, rut.length - 2);
document.getElementById("Cuenta").setAttribute("value", ruty);