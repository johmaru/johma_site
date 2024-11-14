// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function typeWriter(element, text, speed) {
    let i = 0;
  function type(){
      if (i < text.length) {
          element.innerHTML += text.charAt(i);
          i++;
          setTimeout(type, speed);
      }
  }
    type();
}

document.addEventListener('DOMContentLoaded', function () {
    const element = document.querySelector('.typewriter');
    const text = element.getAttribute('data-text');
   setTimeout(function () {
        typeWriter(element, text, 100);
    }, 3000);
});