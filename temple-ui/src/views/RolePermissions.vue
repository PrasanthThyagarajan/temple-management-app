<template>
  <div class="page-container">
    <h2>User Role Permissions</h2>

    <div v-if="loading" class="status">Loading...</div>
    <div v-else>
      <div class="toolbar">
        <label>
          Role:
          <select v-model.number="selectedRoleId" @change="loadRolePermissions">
            <option v-for="r in roles" :key="r.roleId" :value="r.roleId">{{ r.roleName }}</option>
          </select>
        </label>
        <button :disabled="saving" @click="save">{{ saving ? 'Saving...' : 'Save Changes' }}</button>
        <span class="status" v-if="message">{{ message }}</span>
      </div>

      <div class="grid">
        <div class="col">
          <h3>Permissions</h3>
          <div class="perm-list">
            <label v-for="p in permissions" :key="p.permissionId" class="perm-item">
              <input type="checkbox" :value="p.permissionId" v-model="selectedPermissionIds" />
              <span class="perm-name">{{ p.permissionName }}</span>
              <span class="perm-desc" v-if="p.description">- {{ p.description }}</span>
            </label>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios'
import { ref, onMounted } from 'vue'
import { useAuth } from '@/stores/auth'

export default {
  name: 'RolePermissions',
  setup() {
    const { hasRole } = useAuth()
    const roles = ref([])
    const permissions = ref([])
    const selectedRoleId = ref(null)
    const selectedPermissionIds = ref([])
    const loading = ref(true)
    const saving = ref(false)
    const message = ref('')

    const fetchRoles = async () => {
      const { data } = await axios.get('/api/roles')
      roles.value = data
      if (roles.value.length && selectedRoleId.value == null) {
        selectedRoleId.value = roles.value[0].roleId
      }
    }

    const fetchPermissions = async () => {
      const { data } = await axios.get('/api/permissions')
      permissions.value = data
    }

    const loadRolePermissions = async () => {
      if (!selectedRoleId.value) return
      const { data } = await axios.get(`/api/roles/${selectedRoleId.value}/permissions`)
      selectedPermissionIds.value = data.permissionIds
    }

    const save = async () => {
      if (!hasRole('Admin')) {
        message.value = 'Only Admin can update role permissions.'
        return
      }
      saving.value = true
      message.value = ''
      try {
        await axios.post(`/api/roles/${selectedRoleId.value}/permissions`, {
          roleId: selectedRoleId.value,
          permissionIds: selectedPermissionIds.value
        })
        message.value = 'Saved successfully.'
      } catch (e) {
        console.error(e)
        message.value = 'Save failed.'
      } finally {
        saving.value = false
      }
    }

    onMounted(async () => {
      try {
        await Promise.all([fetchRoles(), fetchPermissions()])
        await loadRolePermissions()
      } finally {
        loading.value = false
      }
    })

    return {
      roles,
      permissions,
      selectedRoleId,
      selectedPermissionIds,
      loading,
      saving,
      message,
      loadRolePermissions,
      save
    }
  }
}
</script>

<style scoped>
.page-container { padding: 16px; }
.toolbar { display: flex; gap: 12px; align-items: center; margin-bottom: 12px; }
.grid { display: grid; grid-template-columns: 1fr; gap: 16px; }
.perm-list { display: grid; grid-template-columns: 1fr; gap: 8px; }
.perm-item { display: flex; gap: 8px; align-items: center; }
.perm-name { font-weight: 600; }
.status { color: #555; }
</style>
