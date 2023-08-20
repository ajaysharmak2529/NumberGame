if ("serviceWorker" in navigator) {
    navigator.serviceWorker.register("service_worker.js").then(registration => {
        console.log("SW Registered!");
    }).catch(error => {
        console.log("SW Registration Failed");
    });
} else {
    console.log("Not supported");
}
// Cached core static resources 
self.addEventListener("install", e => {
    e.waitUntil(
        caches.open("static").then(cache => {
            return cache.addAll(["./", './images/logo192.png']);
        })
    );
});

// Fatch resources
self.addEventListener("fetch", e => {
    e.respondWith(
        caches.match(e.request).then(response => {
            return response || fetch(e.request);
        })
    );
});

if (!localStorage.getItem('pwaInstallModalShown')) {
    const installModal = new bootstrap.Modal(document.getElementById('installModal'));
    installModal.show();

    const installButton = document.getElementById('installButton');
    let deferredPrompt;

    window.addEventListener('beforeinstallprompt', (event) => {
        // Prevent the default behavior of the browser
        event.preventDefault();

        // Store the event for later use
        deferredPrompt = event;

        installButton.addEventListener('click', () => {
            if (deferredPrompt) {
                // Show the browser's installation prompt
                deferredPrompt.prompt();

                // Wait for the user to respond to the prompt
                deferredPrompt.userChoice.then((choiceResult) => {
                    if (choiceResult.outcome === 'accepted') {
                        console.log('User accepted the install prompt');
                    } else {
                        console.log('User dismissed the install prompt');
                    }

                    // Clear the deferredPrompt after the prompt is shown
                    deferredPrompt = null;
                });

                // Mark the modal as shown
                localStorage.setItem('pwaInstallModalShown', 'true');
                installModal.hide();
            }
        });
    });
}