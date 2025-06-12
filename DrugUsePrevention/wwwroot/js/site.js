// Custom JavaScript cho ứng dụng
document.addEventListener('DOMContentLoaded', function () {
    console.log('Trang đã được tải');

    // Thêm các function JavaScript tùy chỉnh ở đây
    initializeApp();
});

function initializeApp() {
    // Khởi tạo các component
    setupEventListeners();
}

function setupEventListeners() {
    // Thiết lập các event listener
    const buttons = document.querySelectorAll('.btn');
    buttons.forEach(button => {
        button.addEventListener('click', function (e) {
            // Thêm hiệu ứng hoặc xử lý khi click button
            console.log('Button clicked:', this.textContent);
        });
    });
}