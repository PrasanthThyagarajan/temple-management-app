<template>
  <div class="user-registration">
    <h1>User Registration</h1>
    <form @submit.prevent="registerUser">
      <div>
        <label for="email">Email:</label>
        <input type="email" v-model="email" required maxlength="30" />
      </div>
      <div>
        <label for="name">Full Name:</label>
        <input type="text" v-model="name" required maxlength="100" />
      </div>
      <div>
        <label for="password">Password:</label>
        <input type="password" v-model="password" required minlength="6" />
      </div>
      <button type="submit">Register</button>
    </form>
    <p v-if="message">{{ message }}</p>
  </div>
</template>

<script>
import axios from 'axios'
export default {
  data() {
    return {
      email: '',
      name: '',
      password: '',
      message: ''
    };
  },
  methods: {
    async registerUser() {
      try {
        const response = await axios.post('/api/auth/register', {
          fullName: this.name,
          email: this.email,
          password: this.password
        });

        if (response.data && response.data.success) {
          this.message = 'Please check your email to verify your account.';
        } else {
          this.message = (response.data && response.data.message) || 'Registration failed.';
        }
      } catch (error) {
        this.message = 'An error occurred during registration.';
      }
    }
  }
};
</script>

<style scoped>
.user-registration {
  padding: 20px;
}

form div {
  margin-bottom: 10px;
}

label {
  display: block;
  margin-bottom: 5px;
}

input {
  width: 100%;
  padding: 8px;
  box-sizing: border-box;
}

button {
  padding: 10px 15px;
  background-color: #007bff;
  color: white;
  border: none;
  cursor: pointer;
}

button:hover {
  background-color: #0056b3;
}
</style>
