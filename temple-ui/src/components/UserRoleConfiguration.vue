<template>
  <div class="user-role-configuration">
    <h1>User Role Configuration</h1>
    <button @click="createUserRole">Add User Role</button>
    <table>
      <thead>
        <tr>
          <th>User</th>
          <th>Role</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="userRole in userRoles" :key="userRole.userRoleId">
          <td>{{ userRole.user.username }}</td>
          <td>{{ userRole.role.roleName }}</td>
          <td>
            <button @click="editUserRole(userRole)">Edit</button>
            <button @click="deleteUserRole(userRole.userRoleId)">Delete</button>
          </td>
        </tr>
      </tbody>
    </table>
    <div style="margin-top:12px">
      <div v-if="editing">
        <h3>Edit User Role</h3>
        <select v-model="form.userId">
          <option disabled value="">Select User</option>
          <option v-for="u in users" :key="u.userId" :value="u.userId">{{ u.username }} ({{ u.email }})</option>
        </select>
        <select v-model="form.roleId">
          <option disabled value="">Select Role</option>
          <option v-for="r in roles" :key="r.roleId" :value="r.roleId">{{ r.roleName }}</option>
        </select>
        <button @click="saveEdit">Save</button>
        <button @click="cancel">Cancel</button>
      </div>
      <div v-else>
        <h3>Add User Role</h3>
        <select v-model="form.userId">
          <option disabled value="">Select User</option>
          <option v-for="u in users" :key="u.userId" :value="u.userId">{{ u.username }} ({{ u.email }})</option>
        </select>
        <select v-model="form.roleId">
          <option disabled value="">Select Role</option>
          <option v-for="r in roles" :key="r.roleId" :value="r.roleId">{{ r.roleName }}</option>
        </select>
        <button @click="createUserRole">Assign</button>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios'

export default {
  data() {
    return {
      userRoles: [],
      form: { userRoleId: 0, userId: '', roleId: '' },
      users: [],
      roles: [],
      editing: false
    };
  },
  methods: {
    async fetchUserRoles() {
      const { data } = await axios.get('/api/userroles');
      this.userRoles = data;
    },
    async loadUsersAndRoles(){
      const [usersRes, rolesRes] = await Promise.all([
        axios.get('/api/users'),
        axios.get('/api/roles')
      ])
      this.users = usersRes.data
      this.roles = rolesRes.data
    },
    async createUserRole() {
      if (!this.form.userId || !this.form.roleId) return
      await axios.post('/api/userroles', { userId: Number(this.form.userId), roleId: Number(this.form.roleId) })
      this.cancel()
      await this.fetchUserRoles()
    },
    editUserRole(userRole) {
      this.editing = true
      this.form = { userRoleId: userRole.userRoleId, userId: userRole.user.userId, roleId: userRole.role.roleId }
    },
    async saveEdit() {
      await axios.put(`/api/userroles/${this.form.userRoleId}`, { userRoleId: this.form.userRoleId, userId: Number(this.form.userId), roleId: Number(this.form.roleId) })
      this.cancel()
      await this.fetchUserRoles()
    },
    cancel(){
      this.editing = false
      this.form = { userRoleId: 0, userId: '', roleId: '' }
    },
    async deleteUserRole(userRoleId) {
      await axios.delete(`/api/userroles/${userRoleId}`)
      await this.fetchUserRoles()
    }
  },
  async mounted() {
    await Promise.all([this.fetchUserRoles(), this.loadUsersAndRoles()])
  }
};
</script>

<style scoped>
.user-role-configuration {
  padding: 20px;
}

button {
  margin-right: 10px;
}

table {
  width: 100%;
  border-collapse: collapse;
}

table, th, td {
  border: 1px solid black;
}

th, td {
  padding: 10px;
  text-align: left;
}
</style>

