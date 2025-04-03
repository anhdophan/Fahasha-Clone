function ChangeImage(UploadImage, previewImage) {
    if (UploadImage.files && UploadImage.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $(previewImage).attr('src', e.target.result)
        }
        reader.readAsDataURL(UploadImage.files[0]);
    }
}

function isLogin() {
    fetch('/LoginCustomer/CheckLoginStatus') // Assuming the method is in the HomeController
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                throw new Error('Failed to check login status.');
            }
        })
        .then(data => {
            if (data.isLoggedIn) {
                // User is logged in, redirect to profile page
                window.location.href = "/Views/LoginCustomer/Index.cshtml";
            } else {
                // User is not logged in, stay on the login page
                
                window.location.href = "/Views/Product/Index.cshtml";
            }
        })
        .catch(error => {
            console.log('An error occurred while checking login status:', error);
        });
}

