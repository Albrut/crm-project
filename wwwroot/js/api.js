class CRMApi {
    constructor() {
        this.baseUrl = 'http://localhost:5017/api';
        this.token = localStorage.getItem('authToken');
        // Initialize headers if token exists
        if (this.token) {
            this.initializeAuthHeaders();
        }
    }

    initializeAuthHeaders() {
        this.headers = {
            'Authorization': `Bearer ${this.token}`,
            'Content-Type': 'application/json'
        };
    }

    async login(email, password) {
        try {
            const response = await fetch(`${this.baseUrl}/auth/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password })
            });
            
            const data = await response.json();
            
            if (!response.ok) {
                throw new Error(data.message || 'Invalid credentials');
            }
            
            this.token = data.token;
            localStorage.setItem('authToken', data.token);
            this.initializeAuthHeaders();
            return { success: true, data };
        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    async fetchWithAuth(endpoint, options = {}) {
        try {
            if (!this.token) {
                throw new Error('No authentication token');
            }

            const response = await fetch(`${this.baseUrl}${endpoint}`, {
                ...options,
                headers: this.headers
            });

            if (!response.ok) {
                if (response.status === 401) {
                    localStorage.removeItem('authToken');
                    window.location.reload();
                    throw new Error('Authentication expired');
                }
                const errorData = await response.json().catch(() => ({}));
                throw new Error(errorData.message || 'Request failed');
            }

            // Handle empty responses
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            }
            return null;
        } catch (error) {
            console.error('API Error:', error);
            throw error;
        }
    }

    async register(userData) {
        return await fetch(`${this.baseUrl}/auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(userData)
        }).then(res => res.json());
    }

    // Users
    async getAllUsers() {
        return await this.fetchWithAuth('/users');
    }

    async getUserById(id) {
        return await this.fetchWithAuth(`/users/${id}`);
    }

    async getUserPerformance(id) {
        return await this.fetchWithAuth(`/users/${id}/performance`);
    }

    async getCurrentUser() {
        try {
            const response = await this.fetchWithAuth('/auth/current');
            if (!response) {
                throw new Error('No user data received');
            }
            return response;
        } catch (error) {
            console.error('GetCurrentUser Error:', error);
            throw error;
        }
    }

    // Tasks
    async getTasks() {
        return await this.fetchWithAuth('/tasks');
    }

    async createTask(taskData) {
        return await this.fetchWithAuth('/tasks', {
            method: 'POST',
            body: JSON.stringify(taskData)
        });
    }

    async getOverdueTasks() {
        return await this.fetchWithAuth('/tasks/overdue');
    }

    async updateTaskStatus(id, status) {
        return await this.fetchWithAuth(`/tasks/${id}/status`, {
            method: 'PUT',
            body: JSON.stringify(status)
        });
    }

    // Deals
    async getDeals() {
        return await this.fetchWithAuth('/deals');
    }

    async getDealsPipeline() {
        return await this.fetchWithAuth('/deals/pipeline');
    }

    async createDeal(dealData) {
        return await this.fetchWithAuth('/deals', {
            method: 'POST',
            body: JSON.stringify(dealData)
        });
    }

    async updateDealStatus(id, status) {
        return await this.fetchWithAuth(`/deals/${id}/status`, {
            method: 'PUT',
            body: JSON.stringify(status)
        });
    }

    async getDealsAnalytics(startDate, endDate) {
        // Convert dates to UTC ISO string format
        const start = new Date(startDate);
        const end = new Date(endDate);
        return await this.fetchWithAuth(
            `/deals/analytics?startDate=${start.toISOString()}&endDate=${end.toISOString()}`
        );
    }

    // Attendance
    async checkIn(data) {
        return await this.fetchWithAuth('/attendance/check-in', {
            method: 'POST',
            body: JSON.stringify(data)
        });
    }

    async checkOut(data) {
        return await this.fetchWithAuth('/attendance/check-out', {
            method: 'POST',
            body: JSON.stringify(data)
        });
    }
}

// Initialize the application
document.addEventListener('DOMContentLoaded', () => {
    const api = new CRMApi();
    const loginForm = document.getElementById('loginForm');
    const mainApp = document.getElementById('mainApp');
    const navLinks = document.querySelectorAll('.nav-links li');
    const pages = document.querySelectorAll('.page');

    // Login handling
    document.getElementById('login').addEventListener('submit', async (e) => {
        e.preventDefault();
        const form = e.target;
        const submitButton = form.querySelector('button[type="submit"]');
        const errorElement = document.getElementById('loginError');
        
        submitButton.disabled = true;
        errorElement.classList.remove('visible');
        
        const email = document.getElementById('email').value;
        const password = document.getElementById('password').value;
        const response = await api.login(email, password);
        
        if (response.success) {
            try {
                const userInfo = await api.getCurrentUser();
                document.getElementById('userName').textContent = userInfo.name;
                loginForm.classList.add('hidden');
                mainApp.classList.remove('hidden');
                loadDashboard();
            } catch (error) {
                errorElement.textContent = 'Failed to load user data';
                errorElement.classList.add('visible');
                submitButton.disabled = false;
            }
        } else {
            errorElement.textContent = response.error;
            errorElement.classList.add('visible');
            submitButton.disabled = false;
        }
    });

    // Load dashboard data
    async function loadDashboard() {
        const tasks = await api.getTasks();
        const deals = await api.getDeals();
        const today = new Date();
        const startOfYear = new Date(today.getFullYear(), 0, 1);
        const endOfYear = new Date(today.getFullYear(), 11, 31, 23, 59, 59);
        const analytics = await api.getDealsAnalytics(startOfYear, endOfYear);
        
        updateTasksChart(tasks);
        updateDealsChart(analytics);
        
        document.getElementById('tasksStats').innerHTML = `
            <p>Total Tasks: ${tasks.length}</p>
            <p>Overdue: ${tasks.filter(t => new Date(t.dueDate) < new Date()).length}</p>
        `;
        document.getElementById('dealsStats').innerHTML = `
            <p>Total Deals: ${deals.length}</p>
        `;
    }

    function updateTasksChart(tasks) {
        const ctx = document.getElementById('tasksChart').getContext('2d');
        new Chart(ctx, {
            type: 'pie',
            data: {
                labels: ['New', 'In Progress', 'Completed'],
                datasets: [{
                    data: [
                        tasks.filter(t => t.status === 'New').length,
                        tasks.filter(t => t.status === 'In Progress').length,
                        tasks.filter(t => t.status === 'Completed').length
                    ],
                    backgroundColor: ['#e3f2fd', '#fff3e0', '#e8f5e9']
                }]
            }
        });
    }

    // Navigation handling
    navLinks.forEach(link => {
        link.addEventListener('click', () => {
            const pageId = link.getAttribute('data-page');
            pages.forEach(page => {
                page.classList.remove('active');
            });
            document.getElementById(pageId).classList.add('active');
        });
    });

    // Handle modals
    document.getElementById('newTaskBtn').addEventListener('click', () => {
        document.getElementById('taskModal').style.display = 'block';
    });

    document.getElementById('newDealBtn').addEventListener('click', () => {
        document.getElementById('dealModal').style.display = 'block';
    });

    // Form submissions
    document.getElementById('taskForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        const formData = new FormData(e.target);
        await api.createTask(Object.fromEntries(formData));
        document.getElementById('taskModal').style.display = 'none';
        loadDashboard();
    });

    document.getElementById('dealForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        const formData = new FormData(e.target);
        await api.createDeal(Object.fromEntries(formData));
        document.getElementById('dealModal').style.display = 'none';
        loadDashboard();
    });

    // Check authentication on page load
    if (api.token) {
        api.getCurrentUser()
            .then(userInfo => {
                document.getElementById('userName').textContent = userInfo.name;
                loginForm.classList.add('hidden');
                mainApp.classList.remove('hidden');
                loadDashboard();
            })
            .catch(() => {
                localStorage.removeItem('authToken');
            });
    }
});

