// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

$(document).ready(function () {
    // Prevent double submission on forms and show "Đang xử lý..."
    $('form').on('submit', function (e) {
        var $form = $(this);
        
        // Skip for forms that use ajax or custom handling if they have 'no-auto-disable' class
        if ($form.hasClass('no-auto-disable')) {
            return;
        }

        // Check HTML5 validity
        if (this.checkValidity && !this.checkValidity()) {
            return;
        }
        
        // Check jQuery validation if available
        if (typeof $form.valid === 'function' && !$form.valid()) {
            return;
        }

        var $submitBtn = $form.find('button[type="submit"]');
        if ($submitBtn.length > 0 && !$submitBtn.data('is-submitting')) {
            $submitBtn.data('is-submitting', true);
            var originalHtml = $submitBtn.html();
            $submitBtn.data('original-html', originalHtml);
            
            // Disable button visually and change text
            $submitBtn.css('opacity', '0.7');
            $submitBtn.html('Đang xử lý...');
            
            // Prevent further clicks
            $submitBtn.on('click.preventDouble', function(ev) {
                ev.preventDefault();
            });

            // Re-enable after 10 seconds just in case of silent failure
            setTimeout(function() {
                $submitBtn.data('is-submitting', false);
                $submitBtn.css('opacity', '1');
                $submitBtn.html(originalHtml);
                $submitBtn.off('click.preventDouble');
            }, 10000);
        }
    });
});
