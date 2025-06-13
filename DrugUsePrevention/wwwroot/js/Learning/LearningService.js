// LearningService.js - User Learning API Service
class LearningService {
    constructor(baseUrl = '/api/learning') {
        this.API_BASE = baseUrl;
    }

    // Get authentication token
    getAuthToken() {
        return localStorage.getItem('authToken') || sessionStorage.getItem('authToken');
    }

    // Get common headers with authentication
    getHeaders() {
        const headers = {
            'Content-Type': 'application/json'
        };

        const token = this.getAuthToken();
        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        }

        return headers;
    }

    // Lấy dashboard học tập của người dùng
    async getUserDashboard() {
        try {
            const response = await fetch(`${this.API_BASE}/dashboard`, {
                method: 'GET',
                headers: this.getHeaders()
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || `HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();
            console.log('User dashboard response:', data);

            return {
                success: true,
                data: data
            };

        } catch (error) {
            console.error('LearningService.getUserDashboard error:', error);
            return {
                success: false,
                error: error.message,
                data: null
            };
        }
    }

    // Lấy danh sách khóa học đã đăng ký của người dùng
    async getMyRegistrations(options = {}) {
        try {
            const {
                pageIndex = 1,
                pageSize = 10,
                status = null,
                startDate = null,
                endDate = null
            } = options;

            const params = new URLSearchParams({
                pageIndex: pageIndex.toString(),
                pageSize: pageSize.toString()
            });

            if (status) params.append('status', status);
            if (startDate) params.append('startDate', startDate);
            if (endDate) params.append('endDate', endDate);

            const response = await fetch(`${this.API_BASE}/my-courses?${params.toString()}`, {
                method: 'GET',
                headers: this.getHeaders()
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || `HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();
            console.log('My registrations response:', data);

            return {
                success: true,
                data: data
            };

        } catch (error) {
            console.error('LearningService.getMyRegistrations error:', error);
            return {
                success: false,
                error: error.message,
                data: null
            };
        }
    }

    // Cập nhật tiến độ học tập
    async updateProgress(progressData) {
        try {
            const response = await fetch(`${this.API_BASE}/progress`, {
                method: 'PATCH',
                headers: this.getHeaders(),
                body: JSON.stringify(progressData)
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || `HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();
            console.log('Update progress response:', data);

            return {
                success: true,
                data: data
            };

        } catch (error) {
            console.error('LearningService.updateProgress error:', error);
            return {
                success: false,
                error: error.message,
                data: null
            };
        }
    }

    // Lấy thống kê học tập cá nhân
    async getPersonalStats() {
        try {
            // This might be part of the dashboard, but if you need separate endpoint
            const dashboardResult = await this.getUserDashboard();

            if (dashboardResult.success && dashboardResult.data.data) {
                const stats = {
                    totalCourses: dashboardResult.data.data.totalRegisteredCourses || 0,
                    completedCourses: dashboardResult.data.data.completedCourses || 0,
                    inProgressCourses: dashboardResult.data.data.inProgressCourses || 0,
                    averageProgress: dashboardResult.data.data.averageProgress || 0
                };

                return {
                    success: true,
                    data: stats
                };
            }

            return dashboardResult;

        } catch (error) {
            console.error('LearningService.getPersonalStats error:', error);
            return {
                success: false,
                error: error.message,
                data: null
            };
        }
    }

    // Đánh dấu hoàn thành bài học
    async completeLesson(courseId, contentId) {
        try {
            const progressData = {
                courseId: courseId,
                contentId: contentId,
                isCompleted: true,
                completedAt: new Date().toISOString()
            };

            return await this.updateProgress(progressData);

        } catch (error) {
            console.error('LearningService.completeLesson error:', error);
            return {
                success: false,
                error: error.message,
                data: null
            };
        }
    }

    // Cập nhật thời gian học
    async updateStudyTime(courseId, studyTimeMinutes) {
        try {
            const progressData = {
                courseId: courseId,
                studyTimeMinutes: studyTimeMinutes,
                lastAccessedAt: new Date().toISOString()
            };

            return await this.updateProgress(progressData);

        } catch (error) {
            console.error('LearningService.updateStudyTime error:', error);
            return {
                success: false,
                error: error.message,
                data: null
            };
        }
    }

    // Lấy tiến độ học tập của một khóa học cụ thể
    async getCourseProgress(courseId) {
        try {
            // This would typically be part of the course registration data
            const registrationsResult = await this.getMyRegistrations();

            if (registrationsResult.success && registrationsResult.data.data) {
                const courseRegistration = registrationsResult.data.data.find(
                    reg => reg.courseId === courseId
                );

                if (courseRegistration) {
                    return {
                        success: true,
                        data: {
                            courseId: courseRegistration.courseId,
                            progress: courseRegistration.progress || 0,
                            completedLessons: courseRegistration.completedLessons || 0,
                            totalLessons: courseRegistration.totalLessons || 0,
                            lastAccessedAt: courseRegistration.lastAccessedAt,
                            studyTimeMinutes: courseRegistration.studyTimeMinutes || 0
                        }
                    };
                }
            }

            return {
                success: false,
                error: 'Không tìm thấy thông tin đăng ký khóa học',
                data: null
            };

        } catch (error) {
            console.error('LearningService.getCourseProgress error:', error);
            return {
                success: false,
                error: error.message,
                data: null
            };
        }
    }
}