// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function typeWriter(element, text, speed,callback) {
    let i = 0;
  function type(){
      if (i < text.length) {
          element.innerHTML += text.charAt(i);
          i++;
          setTimeout(type, speed);
      } else if (callback) {
          callback();
      }
  }
    type();
}

document.addEventListener('DOMContentLoaded', function () {
    const element = document.querySelectorAll('.typewriter');
    element.forEach(function (el) {
      const text = el.getAttribute('data-text');
      setTimeout(function () {
          typeWriter(el, text, 80, function (){
              const img = el.querySelector('img');
              if (img) {
                  img.style.display = 'inline';
              }
          });
      }, 2000);
    });

    const bodyClass = document.body.className
    const imgElement = document.getElementById('twitter-logo')
    if (bodyClass === 'light-mode'){
        imgElement.src = '/logo-black.png';
    } else if (bodyClass === 'dark-mode'){ {
        imgElement.src = '/logo-white.png';
    }}
  
});