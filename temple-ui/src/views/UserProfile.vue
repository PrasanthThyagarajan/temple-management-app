<template>
  <div class="user-profile-page">
    <UserProfileComponent />
  </div>
</template>

<script setup>
import UserProfileComponent from '../components/UserProfile.vue'
</script>

<style scoped>
.user-profile-page {
  min-height: 100vh;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
}
</style>
