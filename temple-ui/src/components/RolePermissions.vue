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

    <el-table v-if="pageRows.length" :data="pageRows" v-loading="loading" border style="width: 100%" class="perm-table">
      <el-table-column type="index" label="Sl No" width="80" />
      <el-table-column prop="name" label="Page" min-width="200" />
      <el-table-column label="Actions" min-width="260">
        <template #default="scope">
          <el-select v-model="scope.row.selected" multiple placeholder="Select actions" @change="onActionsChange(scope.row)" :disabled="isAdminSelected">
            <el-option v-for="opt in scope.row.options" :key="opt.value" :label="opt.label" :value="opt.value" />
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
      pageRows: [],
      selectedRoleId: null,
      selectedPagePermissionIds: [],
      isAdminSelected: false,
    }
  },
  methods: {
    async loadData() {
      this.loading = true
      try {
        const [rolesRes, permsRes] = await Promise.all([
          axios.get('/api/roles'),
          axios.get('/api/permissions')
        ])
        this.roles = rolesRes.data
        this.permissions = permsRes.data
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
        this.selectedPagePermissionIds = data.pagePermissionIds || []
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
      const permissionLabel = (pid) => {
        switch (pid) {
          case 1: return 'Read'
          case 2: return 'Create'
          case 3: return 'Update'
          case 4: return 'Delete'
          default: return `Permission ${pid}`
        }
      }
      const byPage = {}
      for (const p of this.permissions) {
        const key = p.pageUrl || p.pageName
        if (!byPage[key]) {
          byPage[key] = { key, name: p.pageName || key, options: [] }
        }
        byPage[key].options.push({
          label: permissionLabel(p.permissionId),
          value: p.pagePermissionId
        })
      }
      const rows = Object.values(byPage)
      for (const row of rows) {
        row.selected = row.options
          .map(o => o.value)
          .filter(v => this.selectedPagePermissionIds.includes(v))
      }
      // Sort by page name for stable UI
      rows.sort((a, b) => String(a.name).localeCompare(String(b.name)))
      this.pageRows = rows
    },
    onActionsChange(row) {
      // Recompute the union of all selected page-permission IDs
      const all = []
      for (const r of this.pageRows) {
        if (Array.isArray(r.selected)) all.push(...r.selected)
      }
      this.selectedPagePermissionIds = Array.from(new Set(all))
    },
    async save() {
      if (!this.selectedRoleId) return
      this.saving = true
      try {
        await axios.post(`/api/roles/${this.selectedRoleId}/permissions`, {
          roleId: this.selectedRoleId,
          pagePermissionIds: this.selectedPagePermissionIds
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


