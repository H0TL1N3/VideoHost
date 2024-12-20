import { defineStore } from 'pinia';
import axios from 'axios';

export const useUserStore = defineStore('user', {
  state: () => ({
    user: JSON.parse(localStorage.getItem('user')) || null,
    token: localStorage.getItem('token') || null,
    isAuthenticated: !!localStorage.getItem('token'),
  }),
  actions: {
    async login(credentials) {
      try {
        const response = await axios.post('/api/User/login', credentials);

        console.log(response);

        this.token = response.data.token;
        this.user = response.data.user;
        this.isAuthenticated = true;

        localStorage.setItem('user', JSON.stringify(this.user));
        localStorage.setItem('token', this.token);

        axios.defaults.headers.common['Authorization'] = `Bearer ${this.token}`;

        return response.data; 
      } catch (error) {
        console.error('Login failed:', error);

        // Reset state on error
        this.logout(); 

        throw error;
      }
    },

    async register(userDetails) {
      try {
        const response = await axios.post('/api/User/register', userDetails);

        return response.data; 
      } catch (error) {
        console.error('Registration failed:', error);

        throw error;
      }
    },

    async logout() {
      try {
        await axios.post('/api/User/logout');
      } catch (error) {
        console.error('Logout failed:', error);
      } finally {
        this.user = null;
        this.token = null;
        this.isAuthenticated = false;

        localStorage.removeItem('user');
        localStorage.removeItem('token');

        delete axios.defaults.headers.common['Authorization'];
      }
    },
  },
});
