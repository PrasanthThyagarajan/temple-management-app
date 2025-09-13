<template>
  <div class="role-management">
    <div class="header">
      <h2>Role Management</h2>
      <el-button type="primary" @click="openCreate">Add Role</el-button>
    </div>

    <el-empty v-if="!loading && roles.length === 0" description="No roles found">
      <template #default>
        <el-button type="primary" @click="openCreate">Create your first role</el-button>
      </template>
    </el-empty>

    <el-table v-else :data="roles" v-loading="loading" border style="width: 100%">
      <el-table-column prop="roleName" label="Role Name" min-width="180" />
      <el-table-column prop="description" label="Description" min-width="240" />
      <el-table-column label="Actions" width="180">
        <template #default="scope">
          <el-button size="small" @click="openEdit(scope.row)">Edit</el-button>
          <el-button size="small" type="danger" @click="confirmDelete(scope.row)">Delete</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="dialog.visible" :title="dialog.mode === 'create' ? 'Create Role' : 'Edit Role'" width="480px">
      <el-form :model="form" :rules="rules" ref="formRef" label-width="110px">
        <el-form-item label="Role Name" prop="roleName">
          <el-input v-model="form.roleName" placeholder="Enter role name" />
        </el-form-item>
        <el-form-item label="Description" prop="description">
          <el-input v-model="form.description" placeholder="Enter description" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="closeDialog">Cancel</el-button>
        <el-button type="primary" :loading="saving" @click="submitDialog">
          {{ dialog.mode === 'create' ? 'Create' : 'Save' }}
        </el-button>
      </template>
    </el-dialog>
  </div>
  </template>

<script>
import axios from 'axios'
import { ElMessage, ElMessageBox } from 'element-plus'

export default {
  data() {
    return {
      loading: false,
      saving: false,
      roles: [],
      form: { roleId: 0, roleName: '', description: '' },
      dialog: { visible: false, mode: 'create' },
      rules: {
        roleName: [
          { required: true, message: 'Role name is required', trigger: 'blur' },
          { min: 2, max: 50, message: '2-50 characters', trigger: 'blur' }
        ],
        description: [
          { max: 200, message: 'Up to 200 characters', trigger: 'blur' }
        ]
      }
    };
  },
  methods: {
    async fetchRoles() {
      this.loading = true
      try {
        const { data } = await axios.get('/api/roles')
        this.roles = data
      } catch (e) {
        ElMessage.error('Failed to load roles')
      } finally {
        this.loading = false
      }
    },
    openCreate() {
      this.dialog.mode = 'create'
      this.dialog.visible = true
      this.form = { roleId: 0, roleName: '', description: '' }
    },
    openEdit(role) {
      this.dialog.mode = 'edit'
      this.dialog.visible = true
      this.form = { roleId: role.roleId, roleName: role.roleName, description: role.description || '' }
    },
    closeDialog() {
      this.dialog.visible = false
    },
    async submitDialog() {
      if (!this.form.roleName) return
      this.saving = true
      try {
        if (this.dialog.mode === 'create') {
          await axios.post('/api/roles', { roleName: this.form.roleName, description: this.form.description })
          ElMessage.success('Role created')
        } else {
          await axios.put(`/api/roles/${this.form.roleId}`, { roleId: this.form.roleId, roleName: this.form.roleName, description: this.form.description })
          ElMessage.success('Role updated')
        }
        this.dialog.visible = false
        await this.fetchRoles()
      } catch (e) {
        ElMessage.error('Save failed')
      } finally {
        this.saving = false
      }
    },
    async confirmDelete(role) {
      try {
        await ElMessageBox.confirm(`Delete role "${role.roleName}"?`, 'Confirm', { type: 'warning' })
        await axios.delete(`/api/roles/${role.roleId}`)
        ElMessage.success('Role deleted')
        await this.fetchRoles()
      } catch (_) {}
    }
  },
  mounted() {
    this.fetchRoles();
  }
};
</script>

<style scoped>
.role-management {
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

