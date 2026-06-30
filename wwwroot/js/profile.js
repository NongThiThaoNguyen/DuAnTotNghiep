/* Profile operations */
document.addEventListener("DOMContentLoaded", function () {
    const avatarInput = document.getElementById("avatarInputFile");
    const avatarPreview = document.getElementById("avatarPreviewImage");

    if (avatarInput && avatarPreview) {
        avatarInput.addEventListener("change", function () {
            const file = this.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    avatarPreview.src = e.target.result;
                };
                reader.readAsDataURL(file);
            }
        });
    }
});
