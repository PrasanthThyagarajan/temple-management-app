<template>
  <div style="padding:20px">
    <h2>Verifying your account...</h2>
    <p v-if="message">{{ message }}</p>
  </div>
</template>

<script>
import axios from 'axios'
export default {
  name: 'Verify',
  data(){
    return { message: '' }
  },
  async mounted(){
    const code = this.$route.query.code
    if(!code){
      this.message = 'Missing verification code.'
      return
    }
    try{
      const res = await axios.get(`/api/auth/verify`, { params: { code } })
      if(res.status === 200){
        this.message = 'Account verified. You can login now.'
      } else {
        this.message = 'Invalid or expired verification link.'
      }
    } catch(e){
      this.message = 'Verification failed. Try again later.'
    }
  }
}
</script>
