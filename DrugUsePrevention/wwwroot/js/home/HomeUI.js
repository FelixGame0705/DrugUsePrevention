console.log('Loading HomeUI.js...');

class HomeUI {
    constructor(homeService) {
        console.log('HomeUI constructor');
        this.homeService = homeService;
        this.setup();
    }

    setup() {
        console.log('HomeUI setup');
        setTimeout(() => {
            this.loadData();
        }, 500);
    }

    async loadData() {
        console.log('HomeUI loading data');
        try {
            const stats = await this.homeService.getStatistics();
            console.log('Stats:', stats);

            if (stats.success) {
                this.showStats(stats.data);
            }
        } catch (error) {
            console.error('HomeUI error:', error);
        }
    }

    showStats(data) {
        console.log('Showing stats:', data);

        const elements = [
            { el: document.querySelector('[data-stat="students"] .stat-number'), val: data.totalStudents },
            { el: document.querySelector('[data-stat="courses"] .stat-number'), val: data.totalCourses },
            { el: document.querySelector('[data-stat="completion"] .stat-number'), val: data.completionRate },
            { el: document.querySelector('[data-stat="families"] .stat-number'), val: data.supportedFamilies }
        ];

        elements.forEach(item => {
            if (item.el) {
                this.animate(item.el, item.val);
            }
        });
    }

    animate(element, target) {
        let current = 0;
        const step = target / 60;

        const timer = setInterval(() => {
            current += step;
            if (current >= target) {
                current = target;
                clearInterval(timer);
            }
            element.textContent = Math.floor(current).toLocaleString('vi-VN');
        }, 25);
    }
}

console.log('HomeUI defined');