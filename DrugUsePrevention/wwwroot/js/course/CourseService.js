// CourseService.js - Xử lý tất cả API calls
class CourseService {
    constructor(baseUrl = '/api/courses') {
        this.API_BASE = baseUrl;
    }

    // Get authentication token from localStorage or wherever you store it
    getAuthToken() {
        // Update this based on your authentication implementation
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

    // Lấy danh sách khóa học với pagination và filter
    async getCourses(options = {}) {
        try {
            const {
                pageIndex = 1,
                pageSize = 10,
                startDate = null,
                endDate = null,
                skill = null,
                ageRange = null,
                level = null,
                category = null
            } = options;

            // Build query parameters
            const params = new URLSearchParams({
                pageIndex: pageIndex.toString(),
                pageSize: pageSize.toString()
            });

            if (startDate) params.append('startDate', startDate);
            if (endDate) params.append('endDate', endDate);
            if (skill) params.append('skill', skill);
            if (ageRange) params.append('ageRange', ageRange);
            if (level) params.append('level', level);
            if (category) params.append('category', category);

            const url = `${this.API_BASE}?${params.toString()}`;
            console.log('Calling API:', url);

            const response = await fetch(url, {
                method: 'GET',
                headers: this.getHeaders()
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.error('API Error Response:', errorText);
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();
            console.log('API Response:', data);

            return {
                success: true,
                data: data
            };

        } catch (error) {
            console.error('CourseService.getCourses error:', error);
            return {
                success: false,
                error: error.message,
                data: null
            };
        }
    }

    // Lấy chi tiết một khóa học
    async getCourseById(courseId) {
        try {
            const response = await fetch(`${this.API_BASE}/${courseId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();
            console.log('Course detail response:', data);

            return {
                success: true,
                data: data
            };

        } catch (error) {
            console.error('CourseService.getCourseById error:', error);
            return {
                success: false,
                error: error.message,
                data: null
            };
        }
    }

    // Lấy nội dung khóa học
    async getCourseContents(courseId) {
        try {
            const response = await fetch(`${this.API_BASE}/${courseId}/contents/active`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();
            console.log('Course contents response:', data);

            return {
                success: true,
                data: data
            };

        } catch (error) {
            console.error('CourseService.getCourseContents error:', error);
            return {
                success: false,
                error: error.message,
                data: null
            };
        }
    }
}