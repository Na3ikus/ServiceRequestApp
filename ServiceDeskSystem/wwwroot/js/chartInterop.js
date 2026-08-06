window.chartInterop = {
    statusChart: null,
    priorityChart: null,
    typeChart: null,
    trendChart: null,
    workloadChart: null,
    productChart: null,
    tagsChart: null,

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
                            color: isDark ? '#9ca3af' : '#4b5563',
                            usePointStyle: true,
                            padding: 16,
                            font: { family: "'Inter', sans-serif", size: 12 }
                        }
                    },
                    tooltip: this.getTooltipConfig(isDark)
                },
                cutout: '70%',
                animation: { animateScale: true, animateRotate: true }
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
                            padding: 16,
                            font: { family: "'Inter', sans-serif", size: 12 }
                        }
                    },
                    tooltip: this.getTooltipConfig(isDark)
                },
                cutout: '70%',
                animation: { animateScale: true, animateRotate: true }
            }
        });
    },

    renderTypeChart: function (canvasId, labels, data, backgroundColors) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        if (this.typeChart) {
            this.typeChart.destroy();
        }

        const isDark = document.documentElement.classList.contains('dark');

        this.typeChart = new Chart(ctx, {
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
                            padding: 16,
                            font: { family: "'Inter', sans-serif", size: 12 }
                        }
                    },
                    tooltip: this.getTooltipConfig(isDark)
                },
                cutout: '70%',
                animation: { animateScale: true, animateRotate: true }
            }
        });
    },

    renderTrendChart: function (canvasId, labels, createdData, resolvedData, createdLabel, resolvedLabel) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        if (this.trendChart) {
            this.trendChart.destroy();
        }

        const isDark = document.documentElement.classList.contains('dark');
        const gridColor = isDark ? 'rgba(75, 85, 99, 0.2)' : 'rgba(229, 231, 235, 0.8)';
        const textColor = isDark ? '#9ca3af' : '#6b7280';

        this.trendChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: createdLabel || 'Створено',
                        data: createdData,
                        borderColor: '#3b82f6',
                        backgroundColor: 'rgba(59, 130, 246, 0.12)',
                        borderWidth: 2.5,
                        fill: true,
                        tension: 0.35,
                        pointRadius: 3,
                        pointHoverRadius: 6,
                        pointBackgroundColor: '#3b82f6'
                    },
                    {
                        label: resolvedLabel || 'Вирішено',
                        data: resolvedData,
                        borderColor: '#10b981',
                        backgroundColor: 'rgba(16, 185, 129, 0.12)',
                        borderWidth: 2.5,
                        fill: true,
                        tension: 0.35,
                        pointRadius: 3,
                        pointHoverRadius: 6,
                        pointBackgroundColor: '#10b981'
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    mode: 'index',
                    intersect: false
                },
                plugins: {
                    legend: {
                        position: 'top',
                        align: 'end',
                        labels: {
                            color: textColor,
                            usePointStyle: true,
                            font: { family: "'Inter', sans-serif", size: 12, weight: '500' }
                        }
                    },
                    tooltip: this.getTooltipConfig(isDark)
                },
                scales: {
                    x: {
                        grid: { color: gridColor },
                        ticks: { color: textColor, font: { family: "'Inter', sans-serif", size: 11 } }
                    },
                    y: {
                        beginAtZero: true,
                        grid: { color: gridColor },
                        ticks: {
                            color: textColor,
                            stepSize: 1,
                            precision: 0,
                            font: { family: "'Inter', sans-serif", size: 11 }
                        }
                    }
                }
            }
        });
    },

    renderWorkloadChart: function (canvasId, labels, inProgressData, assignedData, completedData, inProgressLabel, assignedLabel, completedLabel) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        if (this.workloadChart) {
            this.workloadChart.destroy();
        }

        const isDark = document.documentElement.classList.contains('dark');
        const gridColor = isDark ? 'rgba(75, 85, 99, 0.2)' : 'rgba(229, 231, 235, 0.8)';
        const textColor = isDark ? '#9ca3af' : '#6b7280';

        this.workloadChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: inProgressLabel || 'В процесі',
                        data: inProgressData,
                        backgroundColor: '#f59e0b',
                        borderRadius: 6
                    },
                    {
                        label: assignedLabel || 'Призначено (інші)',
                        data: assignedData,
                        backgroundColor: '#3b82f6',
                        borderRadius: 6
                    },
                    {
                        label: completedLabel || 'Виконано',
                        data: completedData,
                        backgroundColor: '#10b981',
                        borderRadius: 6
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top',
                        align: 'end',
                        labels: {
                            color: textColor,
                            usePointStyle: true,
                            font: { family: "'Inter', sans-serif", size: 12 }
                        }
                    },
                    tooltip: this.getTooltipConfig(isDark)
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { color: textColor, font: { family: "'Inter', sans-serif", size: 11 } }
                    },
                    y: {
                        beginAtZero: true,
                        grid: { color: gridColor },
                        ticks: {
                            color: textColor,
                            stepSize: 1,
                            precision: 0,
                            font: { family: "'Inter', sans-serif", size: 11 }
                        }
                    }
                }
            }
        });
    },

    renderProductPerformanceChart: function (canvasId, labels, hoursData, label) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        if (this.productChart) {
            this.productChart.destroy();
        }

        const isDark = document.documentElement.classList.contains('dark');
        const gridColor = isDark ? 'rgba(75, 85, 99, 0.2)' : 'rgba(229, 231, 235, 0.8)';
        const textColor = isDark ? '#9ca3af' : '#6b7280';

        this.productChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: label || 'Середній час (год)',
                    data: hoursData,
                    backgroundColor: 'rgba(99, 102, 241, 0.85)',
                    hoverBackgroundColor: '#6366f1',
                    borderRadius: 8
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        ...this.getTooltipConfig(isDark),
                        callbacks: {
                            label: function (context) {
                                return ` ${context.parsed.x} год (${(context.parsed.x / 24).toFixed(1)} дн)`;
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        beginAtZero: true,
                        grid: { color: gridColor },
                        ticks: {
                            color: textColor,
                            font: { family: "'Inter', sans-serif", size: 11 },
                            callback: function(v) { return v + ' г'; }
                        }
                    },
                    y: {
                        grid: { display: false },
                        ticks: { color: textColor, font: { family: "'Inter', sans-serif", size: 12, weight: '500' } }
                    }
                }
            }
        });
    },

    renderTagDistributionChart: function (canvasId, labels, data, colors) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        if (this.tagsChart) {
            this.tagsChart.destroy();
        }

        const isDark = document.documentElement.classList.contains('dark');

        this.tagsChart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: colors,
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
                            padding: 14,
                            font: { family: "'Inter', sans-serif", size: 12 }
                        }
                    },
                    tooltip: this.getTooltipConfig(isDark)
                },
                cutout: '65%',
                animation: { animateScale: true, animateRotate: true }
            }
        });
    },

    getTooltipConfig: function (isDark) {
        return {
            backgroundColor: isDark ? 'rgba(31, 41, 55, 0.95)' : 'rgba(255, 255, 255, 0.95)',
            titleColor: isDark ? '#f3f4f6' : '#111827',
            bodyColor: isDark ? '#d1d5db' : '#374151',
            borderColor: isDark ? 'rgba(75, 85, 99, 0.5)' : 'rgba(209, 213, 219, 0.8)',
            borderWidth: 1,
            padding: 12,
            cornerRadius: 8,
            titleFont: { size: 13, family: "'Inter', sans-serif", weight: '600' },
            bodyFont: { size: 13, family: "'Inter', sans-serif" },
            displayColors: true,
            boxPadding: 6
        };
    },

    animateCountUp: function (elementId, start, end, duration) {
        let obj = document.getElementById(elementId);
        if (!obj) return;

        let startTimestamp = null;

        const step = (timestamp) => {
            if (!startTimestamp) startTimestamp = timestamp;
            const progress = Math.min((timestamp - startTimestamp) / duration, 1);
            const easeProgress = 1 - Math.pow(1 - progress, 4);

            obj.innerHTML = (Math.floor(easeProgress * (end - start) + start)).toLocaleString();

            if (progress < 1) {
                window.requestAnimationFrame(step);
            } else {
                obj.innerHTML = end.toLocaleString();
            }
        };
        window.requestAnimationFrame(step);
    }
};
