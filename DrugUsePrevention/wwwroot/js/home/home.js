// home.js - Simple version with better error handling

console.log('=== HOME.JS LOADING ===');

// Global variables
let homeService;
let homeUI;

// Check if required classes are loaded
function checkDependencies() {
    const issues = [];

    if (typeof HomeService === 'undefined') {
        issues.push('HomeService class not loaded');
    }

    if (typeof HomeUI === 'undefined') {
        issues.push('HomeUI class not loaded');
    }

    return issues;
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    console.log('=== DOM LOADED ===');
    console.log('Starting home page initialization...');

    // Check dependencies first
    const issues = checkDependencies();
    if (issues.length > 0) {
        console.error('Dependency issues found:', issues);
        showError('Một số file JavaScript chưa tải được: ' + issues.join(', '));
        return;
    }

    try {
        console.log('All dependencies loaded, initializing services...');

        // Initialize services
        homeService = new HomeService();
        console.log('HomeService initialized:', homeService);

        homeUI = new HomeUI(homeService);
        console.log('HomeUI initialized:', homeUI);

        console.log('=== HOME PAGE INITIALIZED SUCCESSFULLY ===');

        // Add basic features
        initializeBasicFeatures();

        // Make services globally available for debugging
        window.homeService = homeService;
        window.homeUI = homeUI;

    } catch (error) {
        console.error('=== INITIALIZATION FAILED ===');
        console.error('Error details:', error);
        console.error('Error stack:', error.stack);
        showError('Không thể khởi tạo trang: ' + error.message);
    }
});

// Initialize basic features
function initializeBasicFeatures() {
    console.log('Initializing basic features...');

    try {
        // Back to top button
        createBackToTopButton();

        // Smooth scrolling
        initializeSmoothScroll();

        console.log('Basic features initialized successfully');

    } catch (error) {
        console.error('Error initializing basic features:', error);
    }
}

// Create back to top button
function createBackToTopButton() {
    console.log('Creating back to top button...');

    const button = document.createElement('button');
    button.innerHTML = '<i class="fas fa-arrow-up"></i>';
    button.className = 'btn btn-primary back-to-top';
    button.setAttribute('aria-label', 'Về đầu trang');
    button.style.cssText = `
        position: fixed;
        bottom: 20px;
        right: 20px;
        width: 50px;
        height: 50px;
        border-radius: 50%;
        display: none;
        z-index: 1000;
        box-shadow: 0 4px 12px rgba(0,0,0,0.3);
        transition: all 0.3s ease;
    `;

    document.body.appendChild(button);

    // Show/hide on scroll
    window.addEventListener('scroll', function () {
        if (window.pageYOffset > 300) {
            button.style.display = 'block';
        } else {
            button.style.display = 'none';
        }
    });

    // Click handler
    button.addEventListener('click', function () {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });

    console.log('Back to top button created');
}

// Initialize smooth scroll
function initializeSmoothScroll() {
    console.log('Initializing smooth scroll...');

    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    console.log('Smooth scroll initialized');
}

// Show error message
function showError(message) {
    console.error('Showing error:', message);

    const errorDiv = document.createElement('div');
    errorDiv.className = 'alert alert-danger alert-dismissible position-fixed';
    errorDiv.style.cssText = `
        top: 20px; 
        left: 50%; 
        transform: translateX(-50%); 
        z-index: 9999; 
        max-width: 90%;
        box-shadow: 0 4px 12px rgba(220, 53, 69, 0.3);
    `;
    errorDiv.innerHTML = `
        <div class="d-flex align-items-center">
            <i class="fas fa-exclamation-triangle me-2"></i>
            <div>
                <strong>Lỗi khởi tạo!</strong><br>
                <small>${message}</small>
            </div>
        </div>
        <button type="button" class="btn-close" onclick="this.parentElement.remove()"></button>
    `;

    document.body.appendChild(errorDiv);

    // Auto remove after 10 seconds
    setTimeout(() => {
        if (errorDiv.parentElement) {
            errorDiv.remove();
        }
    }, 10000);
}

// Global utility object
window.HomePageUtils = {
    showToast: function (message, type = 'success') {
        console.log('Showing toast:', message, type);

        const toast = document.createElement('div');
        toast.className = `alert alert-${type === 'success' ? 'success' : 'danger'} alert-dismissible position-fixed`;
        toast.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        toast.innerHTML = `
            <div class="d-flex align-items-center">
                <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-triangle'} me-2"></i>
                <span>${message}</span>
            </div>
            <button type="button" class="btn-close" onclick="this.parentElement.remove()"></button>
        `;

        document.body.appendChild(toast);

        // Auto remove after 5 seconds
        setTimeout(() => {
            if (toast.parentElement) {
                toast.remove();
            }
        }, 5000);
    },

    formatNumber: function (num) {
        return new Intl.NumberFormat('vi-VN').format(num);
    },

    refresh: function () {
        console.log('Refreshing page...');
        if (homeUI && homeUI.loadData) {
            homeUI.loadData();
        } else {
            window.location.reload();
        }
    },

    // Debug functions
    debug: {
        checkServices: function () {
            console.log('HomeService:', window.homeService);
            console.log('HomeUI:', window.homeUI);
            console.log('HomeService type:', typeof HomeService);
            console.log('HomeUI type:', typeof HomeUI);
        },

        testAPI: function () {
            if (window.homeService) {
                console.log('Testing HomeService API...');
                window.homeService.getStatistics().then(result => {
                    console.log('Statistics result:', result);
                });
                window.homeService.getFeaturedCourses().then(result => {
                    console.log('Featured courses result:', result);
                });
            } else {
                console.error('HomeService not available');
            }
        }
    }
};

console.log('=== HOME.JS SETUP COMPLETE ===');

// Add window error handler for debugging
window.addEventListener('error', function (e) {
    console.error('=== WINDOW ERROR ===');
    console.error('Message:', e.message);
    console.error('Source:', e.filename);
    console.error('Line:', e.lineno);
    console.error('Column:', e.colno);
    console.error('Error object:', e.error);
});

// Add unhandled promise rejection handler
window.addEventListener('unhandledrejection', function (e) {
    console.error('=== UNHANDLED PROMISE REJECTION ===');
    console.error('Reason:', e.reason);
    console.error('Promise:', e.promise);
});