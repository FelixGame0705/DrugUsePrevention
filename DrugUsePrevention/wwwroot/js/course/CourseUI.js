// CourseUI.js - UI Component cho courses
class CourseUI {
    constructor(courseService, learningService = null) {
        this.courseService = courseService;
        this.learningService = learningService;
        this.currentPage = 1;
        this.totalPages = 1;
        this.isLoading = false;
        this.currentFilters = {};

        this.initializeElements();
        this.bindEvents();
    }

    // Khởi tạo các DOM elements
    initializeElements() {
        this.elements = {
            // Filter elements
            pageSize: document.getElementById('pageSize'),
            startDate: document.getElementById('startDate'),
            endDate: document.getElementById('endDate'),
            skillFilter: document.getElementById('skillFilter'),
            levelFilter: document.getElementById('levelFilter'),
            categoryFilter: document.getElementById('categoryFilter'),
            loadButton: document.getElementById('loadCourses'),
            clearButton: document.getElementById('clearFilter'),
            refreshButton: document.getElementById('refreshData'),

            // Status elements
            apiStatus: document.getElementById('apiStatus'),
            errorAlert: document.getElementById('errorAlert'),
            errorMessage: document.getElementById('errorMessage'),

            // Table elements
            coursesTableBody: document.getElementById('coursesTableBody'),
            pagination: document.getElementById('pagination'),

            // Modal elements
            courseDetailModal: document.getElementById('courseDetailModal'),
            courseDetailContent: document.getElementById('courseDetailContent')
        };
    }

    // Bind các events
    bindEvents() {
        if (this.elements.loadButton) {
            this.elements.loadButton.addEventListener('click', () => this.loadCourses(1));
        }
        if (this.elements.refreshButton) {
            this.elements.refreshButton.addEventListener('click', () => this.loadCourses(this.currentPage));
        }
        if (this.elements.clearButton) {
            this.elements.clearButton.addEventListener('click', () => this.clearFilters());
        }
    }

    // Load courses với options
    async loadCourses(page = 1) {
        if (this.isLoading) return;

        try {
            this.showLoading();
            this.hideError();

            const options = {
                pageIndex: parseInt(page) || 1,
                pageSize: this.elements.pageSize?.value || 10,
                startDate: this.elements.startDate?.value || null,
                endDate: this.elements.endDate?.value || null,
                skill: this.elements.skillFilter?.value || null,
                level: this.elements.levelFilter?.value || null,
                category: this.elements.categoryFilter?.value || null
            };

            // Store current filters
            this.currentFilters = { ...options };

            const result = await this.courseService.getCourses(options);

            if (result.success) {
                this.displayCourses(result.data);
                this.updatePagination(result.data);
            } else {
                this.showError(`Không thể tải danh sách khóa học: ${result.error}`);
            }

        } catch (error) {
            console.error('CourseUI.loadCourses error:', error);
            this.showError(`Lỗi không xác định: ${error.message}`);
        } finally {
            this.hideLoading();
        }
    }

    // Hiển thị danh sách courses
    displayCourses(apiResponse) {
        const tbody = this.elements.coursesTableBody;
        if (!tbody) return;

        // API trả về structure: { success: true, data: [...], pagination: {...} }
        if (!apiResponse.success || !apiResponse.data || !Array.isArray(apiResponse.data)) {
            tbody.innerHTML = '<tr><td colspan="7" class="text-center text-danger">Không có dữ liệu hoặc lỗi API</td></tr>';
            return;
        }

        const courses = apiResponse.data;

        if (courses.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" class="text-center text-muted">Không có khóa học nào</td></tr>';
            return;
        }

        tbody.innerHTML = courses.map(course => this.renderCourseRow(course)).join('');
    }

    // Render một row của course
    renderCourseRow(course) {
        return `
            <tr>
                <td>${course.courseID || 'N/A'}</td>
                <td>
                    <div class="d-flex align-items-center">
                        ${course.thumbnailUrl ? `<img src="${course.thumbnailUrl}" alt="${course.title}" class="course-thumbnail me-2" style="width: 40px; height: 40px; object-fit: cover; border-radius: 4px;">` : ''}
                        <div>
                            <strong>${course.title || 'N/A'}</strong>
                            ${course.level ? `<br><small class="text-muted">Cấp độ: ${course.level}</small>` : ''}
                        </div>
                    </div>
                </td>
                <td>${this.truncateText(course.description || 'N/A', 100)}</td>
                <td>${this.formatDate(course.createdAt)}</td>
                <td>
                    <span class="badge ${course.isActive ? 'bg-success' : 'bg-secondary'}">
                        ${course.isActive ? 'Hoạt động' : 'Không hoạt động'}
                    </span>
                    ${course.isAccept ? '<span class="badge bg-primary ms-1">Đã duyệt</span>' : '<span class="badge bg-warning ms-1">Chờ duyệt</span>'}
                </td>
                <td>
                    ${course.totalStudents || 0} học viên
                </td>
                <td>
                    <div class="btn-group" role="group">
                        <button class="btn btn-sm btn-outline-primary" onclick="courseUI.viewCourseDetail(${course.courseID})" title="Xem chi tiết">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn btn-sm btn-outline-info" onclick="courseUI.loadCourseContents(${course.courseID})" title="Xem nội dung">
                            <i class="fas fa-list"></i>
                        </button>
                        ${this.renderRegistrationButton(course)}
                        ${this.renderManagementButtons(course)}
                    </div>
                </td>
            </tr>
      
class CourseUI {
    constructor(courseService) {
        this.courseService = courseService;
        this.currentPage = 1;
        this.totalPages = 1;
        this.isLoading = false;
        
        this.initializeElements();
        this.bindEvents();
    }

    // Khởi tạo các DOM elements
    initializeElements() {
        this.elements = {
            // Filter elements
            pageSize: document.getElementById('pageSize'),
            startDate: document.getElementById('startDate'),
            endDate: document.getElementById('endDate'),
            loadButton: document.getElementById('loadCourses'),
            clearButton: document.getElementById('clearFilter'),
            refreshButton: document.getElementById('refreshData'),
            
            // Status elements
            apiStatus: document.getElementById('apiStatus'),
            errorAlert: document.getElementById('errorAlert'),
            errorMessage: document.getElementById('errorMessage'),
            
            // Table elements
            coursesTableBody: document.getElementById('coursesTableBody'),
            pagination: document.getElementById('pagination'),
            
            // Modal elements
            courseDetailModal: document.getElementById('courseDetailModal'),
            courseDetailContent: document.getElementById('courseDetailContent')
        };
    }

    // Bind các events
    bindEvents() {
        this.elements.loadButton.addEventListener('click', () => this.loadCourses(1));
        this.elements.refreshButton.addEventListener('click', () => this.loadCourses(this.currentPage));
        this.elements.clearButton.addEventListener('click', () => this.clearFilters());
    }

    // Load courses với options
    async loadCourses(page = 1) {
        if (this.isLoading) return;

        try {
            this.showLoading();
            this.hideError();

            const options = {
                pageIndex: parseInt(page) || 1,
                pageSize: this.elements.pageSize.value,
                startDate: this.elements.startDate.value || null,
                endDate: this.elements.endDate.value || null
            };

            const result = await this.courseService.getCourses(options);

            if (result.success) {
                this.displayCourses(result.data);
                this.updatePagination(result.data);
            } else {
                this.showError(`Không thể tải danh sách khóa học: ${ result.error } `);
            }

        } catch (error) {
            console.error('CourseUI.loadCourses error:', error);
            this.showError(`Lỗi không xác định: ${ error.message } `);
        } finally {
            this.hideLoading();
        }
    }

    // Hiển thị danh sách courses
    displayCourses(apiResponse) {
        const tbody = this.elements.coursesTableBody;

        if (!apiResponse.success || !apiResponse.data || !Array.isArray(apiResponse.data)) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center text-danger">Không có dữ liệu hoặc lỗi API</td></tr>';
            return;
        }

        const courses = apiResponse.data;

        if (courses.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Không có khóa học nào</td></tr>';
            return;
        }

        tbody.innerHTML = courses.map(course => this.renderCourseRow(course)).join('');
    }

    // Render một row của course
    renderCourseRow(course) {
        return `
            < tr >
                <td>${course.courseID || 'N/A'}</td>
                <td>${course.title || 'N/A'}</td>
                <td>${this.truncateText(course.description || 'N/A', 100)}</td>
                <td>${this.formatDate(course.createdAt)}</td>
                <td>
                    <span class="badge ${course.isActive ? 'bg-success' : 'bg-secondary'}">
                        ${course.isActive ? 'Hoạt động' : 'Không hoạt động'}
                    </span>
                    ${course.isAccept ? '<span class="badge bg-primary ms-1">Đã duyệt</span>' : '<span class="badge bg-warning ms-1">Chờ duyệt</span>'}
                </td>
                <td>
                    <button class="btn btn-sm btn-outline-primary" onclick="courseUI.viewCourseDetail(${course.courseID})">
                        <i class="fas fa-eye"></i> Xem
                    </button>
                    <button class="btn btn-sm btn-outline-info" onclick="courseUI.loadCourseContents(${course.courseID})">
                        <i class="fas fa-list"></i> Nội dung
                    </button>
                </td>
            </tr >
            `;
    }

    // Render nút đăng ký/hủy đăng ký
    renderRegistrationButton(course) {
        // Chỉ hiển thị nút đăng ký nếu khóa học đang hoạt động và đã được duyệt
        if (!course.isActive || !course.isAccept) {
            return '';
        }

        if (course.isRegistered) {
            return `
            < button class="btn btn-sm btn-outline-danger" onclick = "courseUI.unregisterFromCourse(${course.courseID})" title = "Hủy đăng ký" >
                <i class="fas fa-user-minus"></i>
                </button >
            `;
        } else {
            return `
            < button class="btn btn-sm btn-outline-success" onclick = "courseUI.registerForCourse(${course.courseID})" title = "Đăng ký" >
                <i class="fas fa-user-plus"></i>
                </button >
            `;
        }
    }

    // Render nút quản lý (chỉ cho Consultant/Manager)
    renderManagementButtons(course) {
        const userRole = this.getCurrentUserRole();
        
        if (!userRole || !['Consultant', 'Manager'].includes(userRole)) {
            return '';
        }

        let buttons = '';

        // Nút chỉnh sửa (Consultant/Manager)
        buttons += `
            < button class="btn btn-sm btn-outline-warning" onclick = "courseUI.editCourse(${course.courseID})" title = "Chỉnh sửa" >
                <i class="fas fa-edit"></i>
            </button >
            `;

        // Nút xóa (Consultant/Manager)
        buttons += `
            < button class="btn btn-sm btn-outline-dark" onclick = "courseUI.deleteCourse(${course.courseID})" title = "Xóa" >
                <i class="fas fa-trash"></i>
            </button >
            `;

        // Nút phê duyệt (chỉ Manager)
        if (userRole === 'Manager' && !course.isAccept) {
            buttons += `
            < button class="btn btn-sm btn-outline-info" onclick = "courseUI.approveCourse(${course.courseID}, true)" title = "Phê duyệt" >
                <i class="fas fa-check"></i>
                </button >
            `;
        }

        return buttons;
    }

    // Xem chi tiết course
    async viewCourseDetail(courseId) {
        try {
            this.showLoading();

            const result = await this.courseService.getCourseById(courseId);

            if (result.success && result.data.success && result.data.data) {
                const course = result.data.data;
                this.elements.courseDetailContent.innerHTML = this.renderCourseDetail(course);
                this.showModal();
            } else {
                this.showError('Không thể tải chi tiết khóa học');
            }

        } catch (error) {
            this.showError(`Lỗi: ${ error.message } `);
        } finally {
            this.hideLoading();
        }
    }

    // Render chi tiết course
    renderCourseDetail(course) {
        return `
            < div class="row" >
                <div class="col-md-8">
                    ${course.thumbnailUrl ? `
                        <div class="mb-3">
                            <img src="${course.thumbnailUrl}" alt="${course.title}" class="img-fluid rounded" style="max-height: 200px;">
                        </div>
                    ` : ''}
                    
                    <h6>Thông tin cơ bản</h6>
                    <table class="table table-sm">
                        <tr><td><strong>ID:</strong></td><td>${course.courseID}</td></tr>
                        <tr><td><strong>Tên:</strong></td><td>${course.title}</td></tr>
                        <tr><td><strong>Mô tả:</strong></td><td>${course.description}</td></tr>
                        <tr><td><strong>Cấp độ:</strong></td><td>${course.level || 'N/A'}</td></tr>
                        <tr><td><strong>Danh mục:</strong></td><td>${course.category || 'N/A'}</td></tr>
                        <tr><td><strong>Kỹ năng:</strong></td><td>${course.skills ? course.skills.join(', ') : 'N/A'}</td></tr>
                        <tr><td><strong>Độ tuổi phù hợp:</strong></td><td>${course.ageRange || 'N/A'}</td></tr>
                        <tr><td><strong>Thời lượng:</strong></td><td>${course.duration || 'N/A'}</td></tr>
                        <tr><td><strong>Ngày tạo:</strong></td><td>${this.formatDate(course.createdAt)}</td></tr>
                    </table>
                </div>
                <div class="col-md-4">
                    <h6>Trạng thái</h6>
                    <table class="table table-sm">
                        <tr><td><strong>Hoạt động:</strong></td><td>${course.isActive ? 'Có' : 'Không'}</td></tr>
                        <tr><td><strong>Đã duyệt:</strong></td><td>${course.isAccept ? 'Có' : 'Không'}</td></tr>
                        <tr><td><strong>Cập nhật cuối:</strong></td><td>${this.formatDate(course.updatedAt)}</td></tr>
                        <tr><td><strong>Số học viên:</strong></td><td>${course.totalStudents || 0}</td></tr>
                    </table>

                    <div class="mt-3">
                        ${this.renderDetailActionButtons(course)}
                    </div>
                </div>
            </div >
            `;
    }

    // Render action buttons trong modal chi tiết
    renderDetailActionButtons(course) {
        let buttons = '';

        // Nút đăng ký/hủy đăng ký
        if (course.isActive && course.isAccept) {
            if (course.isRegistered) {
                buttons += `
            < button class="btn btn-danger btn-sm me-2" onclick = "courseUI.unregisterFromCourse(${course.courseID})" >
                <i class="fas fa-user-minus me-1"></i>Hủy đăng ký
                    </button >
            `;
            } else {
                buttons += `
            < button class="btn btn-success btn-sm me-2" onclick = "courseUI.registerForCourse(${course.courseID})" >
                <i class="fas fa-user-plus me-1"></i>Đăng ký
                    </button >
            `;
            }
        }

        // Nút xem nội dung
        buttons += `
            < button class="btn btn-info btn-sm me-2" onclick = "courseUI.loadCourseContents(${course.courseID})" >
                <i class="fas fa-list me-1"></i>Xem nội dung
            </button >
            `;

        return buttons;
    }

    // Load course contents
    async loadCourseContents(courseId) {
        try {
            this.showLoading();

            const result = await this.courseService.getActiveCourseContents(courseId);

            if (result.success && result.data.success && result.data.data) {
                const contents = result.data.data;
                this.elements.courseDetailContent.innerHTML = this.renderCourseContents(contents, courseId);
                this.showModal();
            } else {
                this.showError('Không thể tải nội dung khóa học');
            }

        } catch (error) {
            this.showError(`Lỗi: ${ error.message } `);
        } finally {
            this.hideLoading();
        }
    }

    // Render course contents
    renderCourseContents(contents, courseId) {
        let contentHtml = `
            < div class="d-flex justify-content-between align-items-center mb-3" >
                <h6>Nội dung khóa học</h6>
                <button class="btn btn-sm btn-secondary" onclick="courseUI.viewCourseDetail(${courseId})">
                    <i class="fas fa-arrow-left me-1"></i>Quay lại
                </button>
            </div >
            `;

        if (contents.length === 0) {
            contentHtml += '<p class="text-muted">Chưa có nội dung nào</p>';
        } else {
            contentHtml += '<div class="list-group">';
            contents.forEach((content, index) => {
                contentHtml += `
            < div class="list-group-item" >
                <div class="d-flex justify-content-between align-items-start">
                    <div class="flex-grow-1">
                        <h6 class="mb-1">
                            ${index + 1}. ${content.title}
                            ${content.isCompleted ? '<i class="fas fa-check-circle text-success ms-2"></i>' : ''}
                        </h6>
                        <p class="mb-1 text-muted">${content.description || 'Không có mô tả'}</p>
                        <small class="text-muted">
                            Thứ tự: ${content.orderIndex} |
                            Thời lượng: ${content.duration || 'N/A'} |
                            Loại: ${content.contentType || 'N/A'}
                        </small>
                    </div>
                    <div class="btn-group-vertical btn-group-sm">
                        ${content.contentUrl ? `
                                    <button class="btn btn-outline-primary btn-sm" onclick="courseUI.openContent('${content.contentUrl}')" title="Mở nội dung">
                                        <i class="fas fa-play"></i>
                                    </button>
                                ` : ''}
                        ${this.learningService ? `
                                    <button class="btn btn-outline-success btn-sm" onclick="courseUI.markAsCompleted(${courseId}, ${content.contentID})" title="Đánh dấu hoàn thành">
                                        <i class="fas fa-check"></i>
                                    </button>
                                ` : ''}
                    </div>
                </div>
                    </div >
            `;
            });
            contentHtml += '</div>';
        }

        return contentHtml;
    }

    // Đăng ký khóa học
    async registerForCourse(courseId) {
        try {
            const confirmResult = confirm('Bạn có chắc chắn muốn đăng ký khóa học này?');
            if (!confirmResult) return;

            this.showLoading();

            const result = await this.courseService.registerForCourse(courseId);

            if (result.success) {
                this.showSuccess('Đăng ký khóa học thành công!');
                // Refresh current page to update registration status
                await this.loadCourses(this.currentPage);
            } else {
                this.showError(`Không thể đăng ký khóa học: ${ result.error } `);
            }

        } catch (error) {
            this.showError(`Lỗi: ${ error.message } `);
        } finally {
            this.hideLoading();
        }
    }

    // Hủy đăng ký khóa học
    async unregisterFromCourse(courseId) {
        try {
            const confirmResult = confirm('Bạn có chắc chắn muốn hủy đăng ký khóa học này?');
            if (!confirmResult) return;

            this.showLoading();

            const result = await this.courseService.unregisterFromCourse(courseId);

            if (result.success) {
                this.showSuccess('Hủy đăng ký khóa học thành công!');
                // Refresh current page to update registration status
                await this.loadCourses(this.currentPage);
            } else {
                this.showError(`Không thể hủy đăng ký khóa học: ${ result.error } `);
            }

        } catch (error) {
            this.showError(`Lỗi: ${ error.message } `);
        } finally {
            this.hideLoading();
        }
    }

    // Đánh dấu nội dung đã hoàn thành
    async markAsCompleted(courseId, contentId) {
        if (!this.learningService) {
            this.showError('Tính năng học tập không khả dụng');
            return;
        }

        try {
            this.showLoading();

            const result = await this.learningService.completeLesson(courseId, contentId);

            if (result.success) {
                this.showSuccess('Đã đánh dấu hoàn thành!');
                // Refresh content list
                await this.loadCourseContents(courseId);
            } else {
                this.showError(`Không thể cập nhật tiến độ: ${ result.error } `);
            }

        } catch (error) {
            this.showError(`Lỗi: ${ error.message } `);
        } finally {
            this.hideLoading();
        }
    }

    // Mở nội dung học tập
    openContent(contentUrl) {
        if (contentUrl) {
            window.open(contentUrl, '_blank');
        } else {
            this.showError('Liên kết nội dung không hợp lệ');
        }
    }

    // Phê duyệt khóa học (chỉ Manager)
    async approveCourse(courseId, isAccept) {
        try {
            const action = isAccept ? 'phê duyệt' : 'từ chối';
            const confirmResult = confirm(`Bạn có chắc chắn muốn ${ action } khóa học này ? `);
            if (!confirmResult) return;

            this.showLoading();

            const result = await this.courseService.approveCourse(courseId, isAccept);

            if (result.success) {
                this.showSuccess(`${ isAccept ? 'Phê duyệt' : 'Từ chối' } khóa học thành công!`);
                await this.loadCourses(this.currentPage);
            } else {
                this.showError(`Không thể ${ action } khóa học: ${ result.error } `);
            }

        } catch (error) {
            this.showError(`Lỗi: ${ error.message } `);
        } finally {
            this.hideLoading();
        }
    }

    // Xóa khóa học
    async deleteCourse(courseId) {
        try {
            const confirmResult = confirm('Bạn có chắc chắn muốn xóa khóa học này?\nLưu ý: Khóa học sẽ được đánh dấu không hoạt động thay vì xóa hoàn toàn.');
            if (!confirmResult) return;

            this.showLoading();

            const result = await this.courseService.deleteCourse(courseId);

            if (result.success) {
                this.showSuccess('Xóa khóa học thành công!');
                await this.loadCourses(this.currentPage);
            } else {
                this.showError(`Không thể xóa khóa học: ${ result.error } `);
            }

        } catch (error) {
            this.showError(`Lỗi: ${ error.message } `);
        } finally {
            this.hideLoading();
        }
    }

    // Update pagination
    updatePagination(apiResponse) {
        const pagination = this.elements.pagination;
        if (!pagination) return;

        // API response structure: { success: true, data: [...], pagination: {...} }
        if (!apiResponse.pagination) {
            pagination.innerHTML = '';
            return;
        }

        this.currentPage = parseInt(apiResponse.pagination.currentPage) || 1;
        this.totalPages = parseInt(apiResponse.pagination.totalPages) || 1;

        console.log('Pagination info:', { 
            currentPage: this.currentPage, 
            totalPages: this.totalPages, 
            pagination: apiResponse.pagination 
        });

        pagination.innerHTML = this.renderPagination();
    }

    // Render pagination
    renderPagination() {
        let paginationHtml = '';

        // Previous button
        paginationHtml += `
            < li class="page-item ${this.currentPage === 1 ? 'disabled' : ''}" >
                <a class="page-link" href="#" onclick="event.preventDefault(); courseUI.loadCourses(${Math.max(1, this.currentPage - 1)})">Trước</a>
            </li >
            `;

        // Page numbers
        const startPage = Math.max(1, this.currentPage - 2);
        const endPage = Math.min(this.totalPages, this.currentPage + 2);

        for (let i = startPage; i <= endPage; i++) {
            paginationHtml += `
            < li class="page-item ${i === this.currentPage ? 'active' : ''}" >
                <a class="page-link" href="#" onclick="event.preventDefault(); courseUI.loadCourses(${i})">${i}</a>
                </li >
            `;
        }

        // Next button  
        paginationHtml += `
            < li class="page-item ${this.currentPage >= this.totalPages ? 'disabled' : ''}" >
                <a class="page-link" href="#" onclick="event.preventDefault(); courseUI.loadCourses(${Math.min(this.totalPages, this.currentPage + 1)})">Sau</a>
            </li >
            `;

        return paginationHtml;
    }

    // Clear filters
    clearFilters() {
        if (this.elements.startDate) this.elements.startDate.value = '';
        if (this.elements.endDate) this.elements.endDate.value = '';
        if (this.elements.pageSize) this.elements.pageSize.value = '10';
        if (this.elements.skillFilter) this.elements.skillFilter.value = '';
        if (this.elements.levelFilter) this.elements.levelFilter.value = '';
        if (this.elements.categoryFilter) this.elements.categoryFilter.value = '';
    }

    // Get current user role (implement based on your auth system)
    getCurrentUserRole() {
        // This should be implemented based on your authentication system
        // For example, from JWT token or user session
        try {
            const token = localStorage.getItem('authToken') || sessionStorage.getItem('authToken');
            if (token) {
                // Decode JWT token to get user role
                const payload = JSON.parse(atob(token.split('.')[1]));
                return payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
            }
        } catch (error) {
            console.error('Error getting user role:', error);
        }
        return null;
    }

    // UI State methods
    showLoading() {
        this.isLoading = true;
        if (this.elements.apiStatus) {
            this.elements.apiStatus.classList.remove('d-none');
        }
    }

    hideLoading() {
        this.isLoading = false;
        if (this.elements.apiStatus) {
            this.elements.apiStatus.classList.add('d-none');
        }
    }

    showError(message) {
        if (this.elements.errorMessage && this.elements.errorAlert) {
            this.elements.errorMessage.textContent = message;
            this.elements.errorAlert.classList.remove('d-none');
            
            // Auto hide after 5 seconds
            setTimeout(() => {
                this.elements.errorAlert.classList.add('d-none');
            }, 5000);
        } else {
            alert(message); // Fallback
        }
    }

    showSuccess(message) {
        // Create success toast
        const toast = document.createElement('div');
        toast.className = 'toast align-items-center text-white bg-success border-0 position-fixed';
        toast.style.cssText = 'top: 20px; right: 20px; z-index: 9999;';
        toast.innerHTML = `
            < div class="d-flex" >
                <div class="toast-body">
                    <i class="fas fa-check-circle me-2"></i>${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div >
            `;
        
        document.body.appendChild(toast);
        
        const bsToast = new bootstrap.Toast(toast);
        bsToast.show();
        
        toast.addEventListener('hidden.bs.toast', () => {
            if (toast.parentElement) {
                toast.remove();
            }
        });
    }

    hideError() {
        if (this.elements.errorAlert) {
            this.elements.errorAlert.classList.add('d-none');
        }
    }

    showModal() {
        if (this.elements.courseDetailModal) {
            const modal = new bootstrap.Modal(this.elements.courseDetailModal);
            modal.show();
        }
    }

    // Utility methods
    formatDate(dateString) {
        if (!dateString) return 'N/A';
        return new Date(dateString).toLocaleString('vi-VN');
    }

    truncateText(text, maxLength) {
        if (!text || text.length <= maxLength) return text;
        return text.substring(0, maxLength) + '...';
    }
}

    // Xem chi tiết course
    async viewCourseDetail(courseId) {
        try {
            this.showLoading();

            const result = await this.courseService.getCourseById(courseId);

            if (result.success && result.data.success && result.data.data) {
                const course = result.data.data;
                this.elements.courseDetailContent.innerHTML = this.renderCourseDetail(course);
                this.showModal();
            } else {
                this.showError('Không thể tải chi tiết khóa học');
            }

        } catch (error) {
            this.showError(`Lỗi: ${ error.message } `);
        } finally {
            this.hideLoading();
        }
    }

    // Render chi tiết course
    renderCourseDetail(course) {
        return `
            < div class="row" >
                <div class="col-md-6">
                    <h6>Thông tin cơ bản</h6>
                    <p><strong>ID:</strong> ${course.courseID}</p>
                    <p><strong>Tên:</strong> ${course.title}</p>
                    <p><strong>Mô tả:</strong> ${course.description}</p>
                    <p><strong>Ngày tạo:</strong> ${this.formatDate(course.createdAt)}</p>
                </div>
                <div class="col-md-6">
                    <h6>Trạng thái</h6>
                    <p><strong>Hoạt động:</strong> ${course.isActive ? 'Có' : 'Không'}</p>
                    <p><strong>Đã duyệt:</strong> ${course.isAccept ? 'Có' : 'Không'}</p>
                    <p><strong>Cập nhật cuối:</strong> ${this.formatDate(course.updatedAt)}</p>
                </div>
            </div >
            `;
    }

    // Load course contents
    async loadCourseContents(courseId) {
        try {
            this.showLoading();

            const result = await this.courseService.getCourseContents(courseId);

            if (result.success && result.data.success && result.data.data) {
                const contents = result.data.data;
                this.elements.courseDetailContent.innerHTML = this.renderCourseContents(contents);
                this.showModal();
            } else {
                this.showError('Không thể tải nội dung khóa học');
            }

        } catch (error) {
            this.showError(`Lỗi: ${ error.message } `);
        } finally {
            this.hideLoading();
        }
    }

    // Render course contents
    renderCourseContents(contents) {
        let contentHtml = '<h6>Nội dung khóa học</h6>';

        if (contents.length === 0) {
            contentHtml += '<p class="text-muted">Chưa có nội dung nào</p>';
        } else {
            contentHtml += '<ul class="list-group">';
            contents.forEach(content => {
                contentHtml += `
            < li class="list-group-item" >
                        <strong>${content.title}</strong>
                        <p class="mb-1 text-muted">${content.description || 'Không có mô tả'}</p>
                        <small>Thứ tự: ${content.orderIndex}</small>
                    </li >
            `;
            });
            contentHtml += '</ul>';
        }

        return contentHtml;
    }

    // Update pagination
    updatePagination(apiResponse) {
        const pagination = this.elements.pagination;

        if (!apiResponse.pagination) {
            pagination.innerHTML = '';
            return;
        }

        this.currentPage = parseInt(apiResponse.pagination.currentPage) || 1;
        this.totalPages = parseInt(apiResponse.pagination.totalPages) || 1;

        console.log('Pagination info:', { 
            currentPage: this.currentPage, 
            totalPages: this.totalPages, 
            pagination: apiResponse.pagination 
        });

        pagination.innerHTML = this.renderPagination();
    }

    // Render pagination
    renderPagination() {
        let paginationHtml = '';

        // Previous button
        paginationHtml += `
            < li class="page-item ${this.currentPage === 1 ? 'disabled' : ''}" >
                <a class="page-link" href="#" onclick="event.preventDefault(); courseUI.loadCourses(${Math.max(1, this.currentPage - 1)})">Trước</a>
            </li >
            `;

        // Page numbers
        for (let i = Math.max(1, this.currentPage - 2); i <= Math.min(this.totalPages, this.currentPage + 2); i++) {
            paginationHtml += `
            < li class="page-item ${i === this.currentPage ? 'active' : ''}" >
                <a class="page-link" href="#" onclick="event.preventDefault(); courseUI.loadCourses(${i})">${i}</a>
                </li >
            `;
        }

        // Next button  
        paginationHtml += `
            < li class="page-item ${this.currentPage >= this.totalPages ? 'disabled' : ''}" >
                <a class="page-link" href="#" onclick="event.preventDefault(); courseUI.loadCourses(${Math.min(this.totalPages, this.currentPage + 1)})">Sau</a>
            </li >
            `;

        return paginationHtml;
    }

    // Clear filters
    clearFilters() {
        this.elements.startDate.value = '';
        this.elements.endDate.value = '';
        this.elements.pageSize.value = '10';
    }

    // UI State methods
    showLoading() {
        this.isLoading = true;
        this.elements.apiStatus.classList.remove('d-none');
    }

    hideLoading() {
        this.isLoading = false;
        this.elements.apiStatus.classList.add('d-none');
    }

    showError(message) {
        this.elements.errorMessage.textContent = message;
        this.elements.errorAlert.classList.remove('d-none');
    }

    hideError() {
        this.elements.errorAlert.classList.add('d-none');
    }

    showModal() {
        const modal = new bootstrap.Modal(this.elements.courseDetailModal);
        modal.show();
    }

    // Utility methods
    formatDate(dateString) {
        if (!dateString) return 'N/A';
        return new Date(dateString).toLocaleString('vi-VN');
    }

    truncateText(text, maxLength) {
        if (text.length <= maxLength) return text;
        return text.substring(0, maxLength) + '...';
    }
}