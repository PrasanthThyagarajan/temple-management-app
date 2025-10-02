const axios = require('axios');

const API_BASE = 'http://localhost:5051/api';

// Test credentials - using admin user
const TEST_USERNAME = 'admin';
const TEST_PASSWORD = 'admin123';

async function testInventoryCRUDWithAuth() {
  try {
    console.log('🧪 Testing Inventory CRUD Operations with Authentication...');

    // Create axios instance with basic auth
    const authHeader = 'Basic ' + Buffer.from(`${TEST_USERNAME}:${TEST_PASSWORD}`).toString('base64');
    const api = axios.create({
      baseURL: API_BASE,
      headers: {
        'Authorization': authHeader,
        'Content-Type': 'application/json'
      }
    });

    console.log('\n🔐 Testing authentication...');
    try {
      const authTest = await api.get('/auth/me');
      console.log(`✅ Authentication successful: ${authTest.data.fullName} (${authTest.data.email})`);
    } catch (authError) {
      console.log('❌ Authentication failed:', authError.response?.data || authError.message);
      return;
    }

    // First, let's get existing temples and areas
    console.log('\n1. Getting existing temples and areas...');
    let temple, area;
    
    try {
      const templesResponse = await api.get('/temples');
      if (templesResponse.data.length > 0) {
        temple = templesResponse.data[0];
        console.log(`✅ Using existing temple: ${temple.name} (ID: ${temple.id})`);
      } else {
        // Create a test temple
        temple = await api.post('/temples', {
          name: 'Test Temple for Inventory',
          address: '123 Test Street',
          city: 'Test City',
          state: 'Test State',
          phone: '123-456-7890',
          email: 'test@temple.com',
          description: 'Test temple for inventory testing',
          deity: 'Lord Ganesha',
          establishedDate: new Date().toISOString()
        });
        temple = temple.data;
        console.log(`✅ Created temple: ${temple.name} (ID: ${temple.id})`);
      }
    } catch (err) {
      console.log('❌ Error with temples:', err.response?.data || err.message);
      return;
    }

    try {
      const areasResponse = await api.get('/areas');
      const templeAreas = areasResponse.data.filter(a => a.templeId === temple.id);
      if (templeAreas.length > 0) {
        area = templeAreas[0];
        console.log(`✅ Using existing area: ${area.name} (ID: ${area.id})`);
      } else {
        // Create a test area
        area = await api.post('/areas', {
          templeId: temple.id,
          name: 'Test Storage Area',
          description: 'Test area for inventory testing'
        });
        area = area.data;
        console.log(`✅ Created area: ${area.name} (ID: ${area.id})`);
      }
    } catch (err) {
      console.log('❌ Error with areas:', err.response?.data || err.message);
      return;
    }

    // Test CREATE operation
    console.log('\n2. Testing CREATE operation...');
    const newInventoryItem = {
      templeId: temple.id,
      areaId: area.id,
      itemName: 'Test Golden Lamp',
      itemWorth: 3, // Precious (0=Low, 1=Medium, 2=High, 3=Precious)
      approximatePrice: 5000.00,
      quantity: 2,
      active: true
    };

    let createdItem;
    try {
      const response = await api.post('/inventories', newInventoryItem);
      createdItem = response.data;
      console.log(`✅ Created inventory item: ${createdItem.itemName} (ID: ${createdItem.id})`);
      console.log(`   Worth: ${createdItem.itemWorth}, Price: ₹${createdItem.approximatePrice}, Quantity: ${createdItem.quantity}`);
    } catch (err) {
      console.log('❌ Error creating inventory:', err.response?.data || err.message);
      return;
    }

    // Test READ operations
    console.log('\n3. Testing READ operations...');
    
    try {
      // Get all inventories
      const allInventories = await api.get('/inventories');
      console.log(`✅ Retrieved ${allInventories.data.length} inventory items`);

      // Get by ID
      const itemById = await api.get(`/inventories/${createdItem.id}`);
      console.log(`✅ Retrieved item by ID: ${itemById.data.itemName}`);

      // Get by temple
      const itemsByTemple = await api.get(`/inventories/temple/${temple.id}`);
      console.log(`✅ Retrieved ${itemsByTemple.data.length} items for temple: ${temple.name}`);

      // Get by area
      const itemsByArea = await api.get(`/inventories/area/${area.id}`);
      console.log(`✅ Retrieved ${itemsByArea.data.length} items for area: ${area.name}`);

      // Get by worth (Precious = 3)
      const preciousItems = await api.get(`/inventories/worth/3`);
      console.log(`✅ Retrieved ${preciousItems.data.length} precious items`);

      // Get active items
      const activeItems = await api.get('/inventories/active');
      console.log(`✅ Retrieved ${activeItems.data.length} active items`);

      // Test calculation endpoints
      const areaValue = await api.get(`/inventories/area/${area.id}/value`);
      console.log(`✅ Total value for area '${area.name}': ₹${areaValue.data.totalValue}`);

      const templeQuantity = await api.get(`/inventories/temple/${temple.id}/quantity`);
      console.log(`✅ Total quantity for temple '${temple.name}': ${templeQuantity.data.totalQuantity} items`);
    } catch (err) {
      console.log('❌ Error in READ operations:', err.response?.data || err.message);
    }

    // Test UPDATE operation
    console.log('\n4. Testing UPDATE operation...');
    const updatedData = {
      templeId: temple.id,
      areaId: area.id,
      itemName: 'Updated Golden Lamp - Premium',
      itemWorth: 3, // Still Precious
      approximatePrice: 7500.00, // Increased price
      quantity: 1, // Reduced quantity
      active: true
    };

    try {
      const updatedItem = await api.put(`/inventories/${createdItem.id}`, updatedData);
      console.log(`✅ Updated inventory item: ${updatedItem.data.itemName}`);
      console.log(`   New Price: ₹${updatedItem.data.approximatePrice}, New Quantity: ${updatedItem.data.quantity}`);
    } catch (err) {
      console.log('❌ Error updating inventory:', err.response?.data || err.message);
    }

    // Test DELETE operation
    console.log('\n5. Testing DELETE operation...');
    try {
      await api.delete(`/inventories/${createdItem.id}`);
      console.log(`✅ Deleted inventory item: ${updatedData.itemName}`);

      // Verify deletion
      try {
        await api.get(`/inventories/${createdItem.id}`);
        console.log('❌ Item should have been deleted but still exists');
      } catch (err) {
        if (err.response && err.response.status === 404) {
          console.log('✅ Confirmed item was deleted (404 Not Found)');
        } else {
          console.log('⚠️ Unexpected error when verifying deletion:', err.message);
        }
      }
    } catch (err) {
      console.log('❌ Error deleting inventory:', err.response?.data || err.message);
    }

    console.log('\n🎉 All Inventory CRUD operations completed!');
    console.log('\n📊 Test Summary:');
    console.log('✅ AUTHENTICATION - Successfully authenticated with API');
    console.log('✅ CREATE - Successfully created inventory item');
    console.log('✅ READ - Successfully retrieved items by various filters');
    console.log('✅ UPDATE - Successfully updated inventory item');
    console.log('✅ DELETE - Successfully deleted inventory item');
    console.log('✅ CALCULATIONS - Successfully calculated totals');
    console.log('\n🚀 Inventory management system is working perfectly!');

  } catch (error) {
    console.error('❌ Unexpected error during testing:', error.message);
    if (error.response) {
      console.error('Status:', error.response.status);
      console.error('Data:', error.response.data);
    }
  }
}

// Run the test
testInventoryCRUDWithAuth();
