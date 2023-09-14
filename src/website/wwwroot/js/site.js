// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function signup(role, id)
{
    // var message = `Will sign up for ${role} in ride ${id}`;
    // alert(message);

    const event = new CustomEvent("signup", {  detail: {role: role, rideId: id }});
    document.dispatchEvent(event);
}

function dropout(role, id)
{
    const event = new CustomEvent("dropout", {  detail: {role: role, rideId: id }});
    document.dispatchEvent(event);
}


function toggleClass(id, className)
{
    var element = document.getElementById(id);
    if(element.classList.contains(className))
    {
        element.classList.remove(className);
    }
    else
    {
        element.classList.add(className);
    }   
}

function printId(id)
{
    console.log("Printing ", id)
    printElement2( document.getElementById(id))
}


function printElement(elem) {
    console.log("cloning dom of target element")
    var domClone = elem.cloneNode(true);
    
    console.log("getting printSection")
    var $printSection = document.getElementById("printSection");
    
    if (!$printSection) {
        console.log("printSection not found, creating ne div")
        var $printSection = document.createElement("div");
        $printSection.id = "printSection";
        document.body.appendChild($printSection);
    }
    
    console.log("clear innerHTML of printSection")
    $printSection.innerHTML = "";
    console.log("adding domClone to printSection")
    $printSection.appendChild(domClone);

    console.log("calling window.print()")
    window.print();
}


function printElement2(elem) {
    console.log("backing up body html")
    var bodyBack = document.body.innerHTML;

    console.log("cloning dom of target element")
    var domClone = elem.cloneNode(true);
    
    console.log("creating print section")
    var $printSection = document.createElement("div");
    $printSection.id = "printSection";
    document.body.innerHTML=""
    document.body.appendChild($printSection);
    
    console.log("adding domClone to printSection")
    $printSection.appendChild(domClone);

    console.log("calling window.print()")
    window.print();

    document.body.innerHTML = bodyBack;
}