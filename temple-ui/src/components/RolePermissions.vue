<template>
  <div class="role-permissions">
    <div class="header">
      <h2>Role Permissions</h2>
      <div class="actions">
        <el-select v-model="selectedRoleId" placeholder="Select role" filterable style="min-width: 260px" @change="loadRolePermissions">
          <el-option v-for="r in roles" :key="r.roleId" :label="r.roleName" :value="r.roleId" />
        </el-select>
        <el-button type="primary" :disabled="!selectedRoleId || isAdminSelected" :loading="saving" @click="save">
          Save Changes
        </el-button>
      </div>
    </div>

    <el-table v-if="pages.length" :data="pageRows" v-loading="loading" border style="width: 100%" class="perm-table">
      <el-table-column type="index" label="Sl No" width="80" />
      <el-table-column prop="name" label="Page" min-width="200" />
      <el-table-column label="Actions" min-width="260">
        <template #default="scope">
          <el-select v-model="scope.row.actions" multiple placeholder="Select actions" @change="onActionsChange(scope.row)" :disabled="isAdminSelected">
            <el-option v-for="opt in actionOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
        </template>
      </el-table-column>
    </el-table>
  </div>
</template>

<script>
import axios from 'axios'
import { ElMessage } from 'element-plus'

export default {
  data() {
    return {
      loading: false,
      saving: false,
      roles: [],
      permissions: [],
      pages: [],
      pageRows: [],
      selectedRoleId: null,
      selectedPermissionIds: [],
      actionOptions: [
        { label: 'View', value: 'view' },
        { label: 'Create', value: 'create' },
        { label: 'Edit', value: 'edit' },
        { label: 'Delete', value: 'delete' }
      ],
      isAdminSelected: false,
    }
  },
  methods: {
    async loadData() {
      this.loading = true
      try {
        const [rolesRes, permsRes, pagesRes] = await Promise.all([
          axios.get('/api/roles'),
          axios.get('/api/permissions'),
          axios.get('/api/config/pages')
        ])
        this.roles = rolesRes.data
        this.permissions = permsRes.data
        this.pages = pagesRes.data
        if (this.roles.length && !this.selectedRoleId) {
          this.selectedRoleId = this.roles[0].roleId
          await this.loadRolePermissions()
        }
      } catch (e) {
        ElMessage.error('Failed to load roles or permissions')
      } finally {
        this.loading = false
      }
    },
    async loadRolePermissions() {
      if (!this.selectedRoleId) return
      this.loading = true
      try {
        const { data } = await axios.get(`/api/roles/${this.selectedRoleId}/permissions`)
        this.selectedPermissionIds = data.permissionIds || []
        this.buildPageRows()
        const selectedRole = this.roles.find(r => r.roleId === this.selectedRoleId)
        this.isAdminSelected = !!selectedRole && String(selectedRole.roleName).toLowerCase() === 'admin'
      } catch (e) {
        ElMessage.error('Failed to load role permissions')
      } finally {
        this.loading = false
      }
    },
    buildPageRows() {
      const permByKey = {}
      for (const p of this.permissions) {
        const parts = (p.permissionName || '').split('.')
        if (parts.length !== 2) continue
        const [pageKey, action] = parts
        if (!permByKey[pageKey]) permByKey[pageKey] = {}
        permByKey[pageKey][action] = p.permissionId
      }
      this.pageRows = this.pages.map((pg) => {
        const key = pg.key
        const mapping = permByKey[key] || {}
        const actions = []
        for (const a of ['view','create','edit','delete']) {
          const pid = mapping[a]
          if (pid && this.selectedPermissionIds.includes(pid)) actions.push(a)
        }
        return { key, name: pg.name, actions }
      })
    },
    onActionsChange(row) {
      const permByKey = {}
      for (const p of this.permissions) {
        const parts = (p.permissionName || '').split('.')
        if (parts.length !== 2) continue
        const [pageKey, action] = parts
        if (!permByKey[pageKey]) permByKey[pageKey] = {}
        permByKey[pageKey][action] = p.permissionId
      }
      const ids = new Set(this.selectedPermissionIds)
      for (const a of ['view','create','edit','delete']) {
        const pid = permByKey[row.key]?.[a]
        if (pid) ids.delete(pid)
      }
      for (const a of row.actions) {
        const pid = permByKey[row.key]?.[a]
        if (pid) ids.add(pid)
      }
      this.selectedPermissionIds = Array.from(ids)
    },
    async save() {
      if (!this.selectedRoleId) return
      this.saving = true
      try {
        await axios.post(`/api/roles/${this.selectedRoleId}/permissions`, {
          roleId: this.selectedRoleId,
          permissionIds: this.selectedPermissionIds
        })
        ElMessage.success('Permissions updated')
      } catch (e) {
        ElMessage.error('Failed to update permissions')
      } finally {
        this.saving = false
      }
    }
  },
  mounted() {
    this.loadData()
  }
}
</script>

<style scoped>
.role-permissions {
  padding: 20px;
}
.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}
.actions {
  display: flex;
  gap: 10px;
}
.permissions-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 16px;
}
.perm-card {
  width: 100%;
}
.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.perm-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 8px 16px;
}
.perm-name {
  font-weight: 600;
}
.perm-desc {
  color: #909399;
  margin-left: 6px;
}
</style>


