if ("serviceWorker" in navigator) {
    navigator.serviceWorker.register("service_worker.js").then(registration => {
        console.log("SW Registered!");
    }).catch(error => {
        console.log("SW Registration Failed");
    });
} else {
    console.log("Not supported");
}

function isAppInstalled() {
    return window.matchMedia('(display-mode: standalone)').matches || window.navigator.standalone;
}

document.addEventListener('DOMContentLoaded', () => {
    const openAppButton = document.getElementById('OpenApp');
    const installButton = document.getElementById('installButton');

    const isPwaInstalled = localStorage.getItem('pwaInstalled') === 'true';
    if (isPwaInstalled) {
        
        openAppButton.style.display = 'block';
        installButton.style.display = 'none';

        openAppButton.addEventListener('click', () => {
            // Redirect to your app's URL
            window.location.href = '/index';
        });
    }
    
    if (!isPwaInstalled) {        
        let deferredPrompt;

        window.addEventListener('beforeinstallprompt', (event) => {
            // Prevent the default behavior of the browser
            event.preventDefault();

            // Store the event for later use
            deferredPrompt = event;

            const installModal = new bootstrap.Modal(document.getElementById('installModal'));
            installModal.show();

            installButton.addEventListener('click', () => {
                if (deferredPrompt) {
                    // Show the browser's installation prompt
                    deferredPrompt.prompt();

                    // Wait for the user to respond to the prompt
                    deferredPrompt.userChoice.then((choiceResult) => {
                        if (choiceResult.outcome === 'accepted') {
                            console.log('User accepted the install prompt');
                    localStorage.setItem('pwaInstalled', true);
                        } else {
                            console.log('User dismissed the install prompt');
                            installModal.show();
                    localStorage.setItem('pwaInstalled', false);
                        }

                        // Clear the deferredPrompt after the prompt is shown
                        deferredPrompt = null;
                    });

                    // Mark the modal as shown
                    installModal.hide();
                }
            });
        });
    }
});
