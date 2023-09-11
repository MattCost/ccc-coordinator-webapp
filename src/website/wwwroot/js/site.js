// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function signup(role, id)
{
    var message = `Will sign up for ${role} in ride ${id}`;
 // create a look event that bubbles up and cannot be canceled

    const event = new CustomEvent("signup", {  detail: {role: role, rideId: id }});
    document.dispatchEvent(event);

    // alert(message);
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
    printElement( document.getElementById(id))
}


function printElement(elem) {
    var domClone = elem.cloneNode(true);
    
    var $printSection = document.getElementById("printSection");
    
    if (!$printSection) {
        var $printSection = document.createElement("div");
        $printSection.id = "printSection";
        document.body.appendChild($printSection);
    }
    
    $printSection.innerHTML = "";
    $printSection.appendChild(domClone);
    window.print();
}