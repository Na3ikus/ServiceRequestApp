window.chartInterop = {
    statusChart: null,
    priorityChart: null,

    renderStatusChart: function (canvasId, labels, data, backgroundColors) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        if (this.statusChart) {
            this.statusChart.destroy();
        }

        const isDark = document.documentElement.classList.contains('dark');

        this.statusChart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: backgroundColors,
                    borderWidth: 0,
                    hoverOffset: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'right',
                        labels: {
                            color: isDark ? '#9ca3af' : '#4b5563', // gray-400 : gray-600
                            usePointStyle: true,
                            padding: 20,
                            font: {
                                family: "'Inter', sans-serif",
                                size: 13
                            }
                        }
                    },
                    tooltip: {
                        backgroundColor: isDark ? 'rgba(31, 41, 55, 0.9)' : 'rgba(255, 255, 255, 0.9)',
                        titleColor: isDark ? '#f3f4f6' : '#111827',
                        bodyColor: isDark ? '#d1d5db' : '#374151',
                        borderColor: isDark ? 'rgba(75, 85, 99, 0.5)' : 'rgba(209, 213, 219, 0.5)',
                        borderWidth: 1,
                        padding: 12,
                        cornerRadius: 8,
                        titleFont: { size: 14, family: "'Inter', sans-serif" },
                        bodyFont: { size: 14, family: "'Inter', sans-serif", weight: 'bold' },
                        displayColors: true,
                        boxPadding: 6
                    }
                },
                cutout: '70%',
                animation: {
                    animateScale: true,
                    animateRotate: true
                }
            }
        });
    },

    renderPriorityChart: function (canvasId, labels, data, backgroundColors) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        if (this.priorityChart) {
            this.priorityChart.destroy();
        }

        const isDark = document.documentElement.classList.contains('dark');

        this.priorityChart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: backgroundColors,
                    borderWidth: 0,
                    hoverOffset: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'right',
                        labels: {
                            color: isDark ? '#9ca3af' : '#4b5563',
                            usePointStyle: true,
                            padding: 20,
                            font: {
                                family: "'Inter', sans-serif",
                                size: 13
                            }
                        }
                    },
                    tooltip: {
                        backgroundColor: isDark ? 'rgba(31, 41, 55, 0.9)' : 'rgba(255, 255, 255, 0.9)',
                        titleColor: isDark ? '#f3f4f6' : '#111827',
                        bodyColor: isDark ? '#d1d5db' : '#374151',
                        borderColor: isDark ? 'rgba(75, 85, 99, 0.5)' : 'rgba(209, 213, 219, 0.5)',
                        borderWidth: 1,
                        padding: 12,
                        cornerRadius: 8,
                        titleFont: { size: 14, family: "'Inter', sans-serif" },
                        bodyFont: { size: 14, family: "'Inter', sans-serif", weight: 'bold' },
                        displayColors: true,
                        boxPadding: 6
                    }
                },
                cutout: '70%',
                animation: {
                    animateScale: true,
                    animateRotate: true
                }
            }
        });
    },

    animateCountUp: function (elementId, start, end, duration) {
        let obj = document.getElementById(elementId);
        if (!obj) return;

        let startTimestamp = null;

        const step = (timestamp) => {
            if (!startTimestamp) startTimestamp = timestamp;
            const progress = Math.min((timestamp - startTimestamp) / duration, 1);

            // easeOutQuart
            const easeProgress = 1 - Math.pow(1 - progress, 4);

            obj.innerHTML = Math.floor(easeProgress * (end - start) + start);

            if (progress < 1) {
                window.requestAnimationFrame(step);
            } else {
                obj.innerHTML = end; // Ensure exact final value
            }
        };
        window.requestAnimationFrame(step);
    }
};
