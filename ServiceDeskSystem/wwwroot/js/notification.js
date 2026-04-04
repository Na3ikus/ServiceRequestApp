window.notificationManager = {
    registerOutsideClick: function (dotNetRef) {
        this.unregisterOutsideClick();

        window.__notificationOutsideClickHandler = function (event) {
            if (event.target.closest('.notification-wrapper')) {
                return;
            }

            dotNetRef.invokeMethodAsync('HandleOutsideNotificationClick');
        };

        document.addEventListener('click', window.__notificationOutsideClickHandler, true);
    },

    unregisterOutsideClick: function () {
        if (!window.__notificationOutsideClickHandler) {
            return;
        }

        document.removeEventListener('click', window.__notificationOutsideClickHandler, true);
        window.__notificationOutsideClickHandler = null;
    },

    playNewNotificationSound: async function () {
        try {
            const AudioContextType = window.AudioContext || window.webkitAudioContext;
            if (!AudioContextType) {
                return;
            }

            if (!window.__notificationAudioContext) {
                window.__notificationAudioContext = new AudioContextType();
            }

            const context = window.__notificationAudioContext;
            if (context.state === 'suspended') {
                await context.resume();
            }

            const oscillator = context.createOscillator();
            const gainNode = context.createGain();

            oscillator.type = 'triangle';
            oscillator.frequency.setValueAtTime(920, context.currentTime);
            oscillator.frequency.exponentialRampToValueAtTime(620, context.currentTime + 0.18);

            gainNode.gain.setValueAtTime(0.0001, context.currentTime);
            gainNode.gain.exponentialRampToValueAtTime(0.07, context.currentTime + 0.02);
            gainNode.gain.exponentialRampToValueAtTime(0.0001, context.currentTime + 0.2);

            oscillator.connect(gainNode);
            gainNode.connect(context.destination);

            oscillator.start(context.currentTime);
            oscillator.stop(context.currentTime + 0.2);
        }
        catch {
            // Ignore browser autoplay or audio API errors.
        }
    }
};
