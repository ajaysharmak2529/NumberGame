// Cached core static resources 
self.addEventListener("install", e => {
    e.waitUntil(
        caches.open("static").then(cache => {
            return cache.addAll(["./", './images/logo192.png']);
        })
    );
});

self.addEventListener('activate', (event) => {
    // Reset the installation flag when the service worker is unregistered
    if (!self.registration.active) {
        localStorage.removeItem('pwaInstalled');
    }
});

// Fatch resources
self.addEventListener("fetch", e => {
    e.respondWith(
        caches.match(e.request).then(response => {
            return response || fetch(e.request);
        })
    );
});