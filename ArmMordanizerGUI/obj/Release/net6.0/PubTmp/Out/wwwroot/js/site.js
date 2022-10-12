// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function RemoveClass(elementId, className) {
    document.getElementById(elementId).className =
        document.getElementById(elementId).className
            .replace(new RegExp('(?:^|\\s)' + className + '(?:\\s|$)'), ' ');
}