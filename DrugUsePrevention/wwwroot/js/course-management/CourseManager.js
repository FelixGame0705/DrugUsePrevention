// Simple CourseManager for testing
console.log('CourseManager.js loading...');

class CourseManager {
    constructor(courseService, learningService) {
        console.log('CourseManager constructor called');
        this.courseService = courseService;
        this.learningService = learningService;
        this.currentPage = 1;
        this.totalPages = 1;

        this.initializeElements();
        console.log('CourseManager initialized successfully');
    }

    initializeElements() {
        this.elements = {
            coursesTableBody: document.getElementById('coursesTableBody'),
            pagination: document.getElementById('pagination'),
            loadingAlert: document.getElementById('loadingAlert'),
            errorAlert: document.getElementById('errorAlert'),
            errorMessage: document.getElementById('errorMessage')
        };
        console.log('Elements initialized:', this.elements);
    }

    // === MAIN METHODS ===

    async loadCourses(page = 1) {
        console.log('Loading courses, page:', page);

        try {
            this.showLoading();
            this.hideError();

            // Simple filter object
            const filters = {
                pageIndex: page,
                pageSize: document.getElementById('pageSize')?.value || 10
            };

            console.log('Calling courseService.getCourses with filters:', filters);
            const result = await this.courseService.getCourses(filters);
            console.log('getCourses result:', result);

            if (result.success && result.data) {
                this.displayCourses(result.data);
                this.updatePagination(result.data);
                this.updateStats(result.data);
            } else {
                this.showError('Không thể tải danh sách khóa học: ' + (result.error || 'Unknown error'));
            }

        } catch (error) {
            console.error('Error loading courses:', error);
            this.showError('Lỗi khi tải khóa học: ' + error.message);
        } finally {
            this.hideLoading();
        }
    }

    displayCourses(apiResponse) {
        console.log('Displaying courses:', apiResponse);
        const tbody = this.elements.coursesTableBody;

        if (!tbody) {
            console.error('Table body not found');
            return;
        }

        if (!apiResponse.success || !apiResponse.data || !Array.isArray(apiResponse.data)) {
            tbody.innerHTML = '<tr><td colspan="8" class="text-center text-danger">Không có dữ liệu hoặc lỗi API</td></tr>';
            return;
        }

        const courses = apiResponse.data;
        if (courses.length === 0) {
            tbody.innerHTML = '<tr><td colspan="8" class="text-center text-muted">Không tìm thấy khóa học nào</td></tr>';
            return;
        }

        tbody.innerHTML = courses.map(course => this.renderCourseRow(course)).join('');
        console.log('Displayed', courses.length, 'courses');
    }

    renderCourseRow(course) {
        return `
            <tr>
                <td>${course.courseID}</td>
                <td>
                    <strong>${course.title}</strong>
                    ${course.level ? `<br><small class="text-muted">Cấp độ: ${course.level}</small>` : ''}
                </td>
                <td>${this.truncateText(course.description || '', 100)}</td>
                <td><span class="badge bg-info">${course.level || 'N/A'}</span></td>
                <td>
                    <span class="badge ${course.isActive ? 'bg-success' : 'bg-secondary'}">
                        ${course.isActive ? 'Hoạt động' : 'Tạm dừng'}
                    </span><br>
                    <span class="badge ${course.isAccept ? 'bg-primary' : 'bg-warning'}">
                        ${course.isAccept ? 'Đã duyệt' : 'Chờ duyệt'}
                    </span>
                </td>
                <td class="text-center">${course.totalStudents || 0}</td>
                <td><small>${this.formatDate(course.createdAt)}</small></td>
                <td>
                    <div class="btn-group-vertical btn-group-sm">
                        <button class="btn btn-outline-info btn-sm" onclick="courseManager.viewCourseDetails(${course.courseID})" title="Chi tiết">
                            <i class="fas fa-eye"></i> Chi tiết
                        </button>
                        <button class="btn btn-outline-primary btn-sm" onclick="courseManager.manageCourseContents(${course.courseID})" title="Nội dung">
                            <i class="fas fa-list"></i> Nội dung
                        </button>
                    </div>
                </td>
            </tr>
        `;
    }

    updatePagination(apiResponse) {
        const pagination = this.elements.pagination;
        if (!pagination) return;

        if (!apiResponse.pagination) {
            pagination.innerHTML = '';
            return;
        }

        this.currentPage = parseInt(apiResponse.pagination.currentPage) || 1;
        this.totalPages = parseInt(apiResponse.pagination.totalPages) || 1;

        let html = '';

        // Previous button
        html += `
            <li class="page-item ${this.currentPage === 1 ? 'disabled' : ''}">
                <a class="page-link" href="#" onclick="event.preventDefault(); courseManager.loadCourses(${Math.max(1, this.currentPage - 1)})">Trước</a>
            </li>
        `;

        // Page numbers
        for (let i = Math.max(1, this.currentPage - 2); i <= Math.min(this.totalPages, this.currentPage + 2); i++) {
            html += `
                <li class="page-item ${i === this.currentPage ? 'active' : ''}">
                    <a class="page-link" href="#" onclick="event.preventDefault(); courseManager.loadCourses(${i})">${i}</a>
                </li>
            `;
        }

        // Next button
        html += `
            <li class="page-item ${this.currentPage >= this.totalPages ? 'disabled' : ''}">
                <a class="page-link" href="#" onclick="event.preventDefault(); courseManager.loadCourses(${Math.min(this.totalPages, this.currentPage + 1)})">Sau</a>
            </li>
        `;

        pagination.innerHTML = html;
        console.log('Pagination updated:', this.currentPage, '/', this.totalPages);
    }

    updateStats(apiResponse) {
        try {
            if (apiResponse.pagination) {
                const total = apiResponse.pagination.totalCount || 0;
                document.getElementById('totalCourses').textContent = total;

                // Mock other stats
                document.getElementById('approvedCourses').textContent = Math.floor(total * 0.8);
                document.getElementById('pendingCourses').textContent = Math.floor(total * 0.2);
                document.getElementById('totalStudents').textContent = total * 10;
            }
        } catch (error) {
            console.error('Error updating stats:', error);
        }
    }

    // === PLACEHOLDER METHODS ===

    showCreateCourseModal() {
        alert('Tính năng tạo khóa học đang được phát triển');
    }

    showMyCoursesModal() {
        alert('Tính năng khóa học của tôi đang được phát triển');
    }

    showDashboard() {
        alert('Tính năng dashboard đang được phát triển');
    }

    viewCourseDetails(courseId) {
        alert('Xem chi tiết khóa học ID: ' + courseId);
    }

    manageCourseContents(courseId) {
        alert('Quản lý nội dung khóa học ID: ' + courseId);
    }

    clearFilters() {
        const filters = ['levelFilter', 'categoryFilter', 'statusFilter', 'startDate', 'endDate'];
        filters.forEach(id => {
            const element = document.getElementById(id);
            if (element) element.value = '';
        });

        const pageSize = document.getElementById('pageSize');
        if (pageSize) pageSize.value = '10';
    }

    refreshData() {
        this.loadCourses(this.currentPage);
    }

    // === UTILITY METHODS ===

    showLoading() {
        if (this.elements.loadingAlert) {
            this.elements.loadingAlert.classList.remove('d-none');
        }
    }

    hideLoading() {
        if (this.elements.loadingAlert) {
            this.elements.loadingAlert.classList.add('d-none');
        }
    }

    showError(message) {
        console.error('Showing error:', message);
        if (this.elements.errorMessage && this.elements.errorAlert) {
            this.elements.errorMessage.textContent = message;
            this.elements.errorAlert.classList.remove('d-none');
        } else {
            alert(message);
        }
    }

    hideError() {
        if (this.elements.errorAlert) {
            this.elements.errorAlert.classList.add('d-none');
        }
    }

    formatDate(dateString) {
        if (!dateString) return 'N/A';
        return new Date(dateString).toLocaleDateString('vi-VN');
    }

    truncateText(text, maxLength) {
        if (!text || text.length <= maxLength) return text;
        return text.substring(0, maxLength) + '...';
    }
}

console.log('CourseManager class defined successfully');

// Make it globally available
window.CourseManager = CourseManager;