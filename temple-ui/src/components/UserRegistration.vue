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
        <label for="gender">Gender:</label>
        <select v-model="gender" required>
          <option value="" disabled>Select Gender</option>
          <option value="Male">Male</option>
          <option value="Female">Female</option>
          <option value="Other">Other</option>
        </select>
      </div>
      <div>
        <label for="password">Password:</label>
        <input type="password" v-model="password" required minlength="6" />
      </div>
      <div>
        <label for="confirmPassword">Confirm Password:</label>
        <input type="password" v-model="confirmPassword" required minlength="6" />
      </div>
      <div>
        <label for="nakshatra">Nakshatra:</label>
        <select v-model="nakshatra">
          <option value="">Select Nakshatra (Optional)</option>
          <option v-for="nakshatraOption in nakshatras" :key="nakshatraOption" :value="nakshatraOption">
            {{ nakshatraOption }}
          </option>
        </select>
      </div>
      <div>
        <label for="dateOfBirth">Date of Birth:</label>
        <input type="datetime-local" v-model="dateOfBirth" />
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
      gender: '',
      password: '',
      confirmPassword: '',
      nakshatra: '',
      dateOfBirth: '',
      message: '',
      nakshatras: [
'Aswathy','Bharani','Karthika','Rohini','Makayiram','Thiruvathira',
'Punartham','Pooyam','Ayilyam','Makam','Pooram','Uthram',
'Hastham','Chithira','Chothi','Visakham','Anizham','Thrikketta',
'Moolam','Pooradam','Uthradam','Thiruvonam','Avittam','Chathayam',
'Pooruruttathi','Uthirattathi','Revathi'
      ]
    };
  },
  methods: {
    async registerUser() {
      try {
        if (this.password !== this.confirmPassword) {
          this.message = 'Passwords do not match.'
          return
        }
        const response = await axios.post('/api/auth/register', {
          fullName: this.name,
          gender: this.gender,
          email: this.email,
          password: this.password,
          confirmPassword: this.confirmPassword,
          nakshatra: this.nakshatra || null,
          dateOfBirth: this.dateOfBirth || null
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

input, select {
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
