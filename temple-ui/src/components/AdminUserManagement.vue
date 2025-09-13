<template>
  <div class="users-container">
    <el-card class="users-card">
      <template #header>
        <div class="card-header">
          <h2>User Management</h2>
          <el-button type="primary" @click="openCreate">
            <el-icon><Plus /></el-icon>
            <span class="btn-text">Add User</span>
          </el-button>
        </div>
      </template>

      <div class="table-container">
        <el-table :data="users" v-loading="loading" stripe style="width: 100%">
          <el-table-column prop="userId" label="ID" width="80" />
          <el-table-column prop="username" label="Username" min-width="160" show-overflow-tooltip />
          <el-table-column prop="email" label="Email" min-width="200" show-overflow-tooltip />
          <el-table-column prop="fullName" label="Full Name" min-width="180" show-overflow-tooltip />
          <el-table-column label="Active" width="100">
            <template #default="scope">
              <el-tag :type="scope.row.isActive ? 'success' : 'warning'" size="small">{{ scope.row.isActive ? 'Yes' : 'No' }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column label="Actions" width="200" fixed="right">
            <template #default="scope">
              <div class="action-buttons">
                <el-button size="small" @click.stop="openEdit(scope.row)">
                  <el-icon><Edit /></el-icon>
                  <span class="btn-text">Edit</span>
                </el-button>
                <el-button size="small" type="danger" @click.stop="confirmDelete(scope.row.userId)">
                  <el-icon><Delete /></el-icon>
                  <span class="btn-text">Delete</span>
                </el-button>
              </div>
            </template>
          </el-table-column>
        </el-table>
      </div>

      <div class="pagination-container">
        <el-pagination
          :current-page="currentPage"
          :page-size="pageSize"
          :total="users.length"
          layout="total, prev, pager, next"
        />
      </div>
    </el-card>

    <el-dialog v-model="dialog.visible" :title="dialog.mode === 'create' ? 'Create User' : 'Edit User'" width="600px">
      <el-form :model="form" :rules="rules" ref="formRef" label-width="120px">
        <el-form-item label="Email" prop="email">
          <el-input v-model="form.email" placeholder="Email" />
        </el-form-item>
        <el-form-item label="Full Name" prop="name">
          <el-input v-model="form.name" placeholder="Full name" />
        </el-form-item>
        <el-form-item label="Password" prop="password">
          <el-input v-model="form.password" type="password" placeholder="Password" :disabled="dialog.mode==='edit'" />
        </el-form-item>
      </el-form>
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="dialog.visible = false">Cancel</el-button>
          <el-button type="primary" :loading="saving" @click="submitDialog">{{ dialog.mode==='create' ? 'Create' : 'Save' }}</el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>

<script>
import axios from 'axios'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Edit, Delete } from '@element-plus/icons-vue'

export default {
  name: 'AdminUserManagement',
  components: { Plus, Edit, Delete },
  data(){
    return {
      loading: false,
      saving: false,
      users: [],
      dialog: { visible: false, mode: 'create' },
      form: { userId: 0, email: '', name: '', password: '' },
      rules: {
        email: [ { required: true, message: 'Email is required', trigger: 'blur' } ],
        name: [ { required: true, message: 'Full name is required', trigger: 'blur' } ],
        password: [ { required: true, message: 'Password is required', trigger: 'blur' } ]
      },
    }
  },
  methods: {
    async load(){
      this.loading = true
      try {
        const { data } = await axios.get('/api/users')
        this.users = data
      } finally {
        this.loading = false
      }
    },
    openCreate(){
      this.dialog.mode = 'create'
      this.dialog.visible = true
      this.form = { userId: 0, email: '', name: '', password: '' }
    },
    openEdit(u){
      this.dialog.mode = 'edit'
      this.dialog.visible = true
      this.form = { userId: u.userId, email: u.email, name: u.fullName, password: '' }
    },
    async submitDialog(){
      this.saving = true
      try {
        if (this.dialog.mode === 'create'){
          await axios.post('/api/users', { email: this.form.email, name: this.form.name, password: this.form.password })
          ElMessage.success('User created')
        } else {
          await axios.put(`/api/users/${this.form.userId}`, { email: this.form.email, name: this.form.name, password: this.form.password })
          ElMessage.success('User updated')
        }
        this.dialog.visible = false
        await this.load()
      } catch (e) {
        ElMessage.error('Operation failed')
      } finally {
        this.saving = false
      }
    },
    async confirmDelete(id){
      try {
        await ElMessageBox.confirm('Delete this user?', 'Confirm', { type: 'warning' })
        await axios.delete(`/api/users/${id}`)
        ElMessage.success('User deleted')
        await this.load()
      } catch(_){}
    }
  },
  async mounted(){
    await this.load()
  }
}
</script>

<style scoped>
.users-container { padding: 20px; }
.users-card { margin-bottom: 20px; }
.card-header { display: flex; justify-content: space-between; align-items: center; }
.table-container { overflow-x: auto; margin-bottom: 20px; }
.action-buttons { display: flex; gap: 5px; flex-wrap: wrap; }
.btn-text { display: inline; }
.pagination-container { text-align: right; }
@media (max-width: 768px){
  .users-container { padding: 10px; }
  .card-header { flex-direction: column; align-items: flex-start; gap: 10px; }
  .btn-text { display: none; }
}
</style>
