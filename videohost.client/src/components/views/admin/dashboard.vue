<template>
  <div class="container mt-5">

    <h1 class="text-center mb-4">Admin Dashboard</h1>

    <div class="mb-4">
      <label for="entitySelector" class="form-label">Select Entity</label>
      <select v-model="selectedEntity" @change="reloadEntities" class="form-select">
        <option value="Tags">Tags</option>
        <option value="Videos">Videos</option>
        <option value="Comments">Comments</option>
        <option value="Users">Users</option>
        <option value="Subscriptions">Subscriptions</option>
      </select>
    </div>

    <table class="table table-striped table-bordered" v-if="entities.length">
      <thead>
        <tr>
          <th v-for="(header, index) in tableHeaders" :key="index">{{ header }}</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(entity, index) in entities" :key="index">
          <td v-for="(value, key) in entity" :key="key">
            {{ value }}
          </td>
          <td>
            <button class="btn btn-warning btn-sm me-2" @click="editEntity(entity.id)">Edit</button>
            <button class="btn btn-danger btn-sm" @click="deleteEntity(entity.id)">Delete</button>
          </td>
        </tr>
      </tbody>
    </table>

    <div class="text-center">
      <button v-if="hasMoreEntities" class="btn btn-outline-secondary" @click="fetchEntities">
        Load More
      </button>
    </div>

    <p v-if="!entities.length" class="text-muted text-center">No {{ selectedEntity }} found.</p>

  </div>
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue';

  import { useHead } from '@unhead/vue';
  import { useToast } from 'vue-toast-notification';
  import { useRouter } from 'vue-router';
  import axios from 'axios';

  import { DEFAULT_TITLE } from '@/assets/const.js';

  useHead({
    title: 'Admin - Dashboard | ' + DEFAULT_TITLE
  })

  const selectedEntity = ref("Tags");;
  const tableHeaders = ref([]);

  const skip = ref(0);
  const take = 10;
  const entities = ref([])
  const hasMoreEntities = ref(true); 

  const router = useRouter();
  const toast = useToast();

  const fetchEntities = async () => {
    try {
      const response = await axios.get('/api/admin/get-entities', {
        params: {
          entityType: selectedEntity.value,
          skip: skip.value,
          take: take,
        },
      });
      entities.value.push(...response.data);

      if (response.data.length < take)
          hasMoreEntities.value = false;

      if (entities.value.length) {
        tableHeaders.value = Object.keys(entities.value[0]);
      } else {
        tableHeaders.value = [];
      }

      skip.value += take;
    } catch (error) {
      toast.error(`Error fetching ${selectedEntity.value}:`, error);
    }
  };

  onMounted(async () => {
    await fetchEntities();
  });

  const reloadEntities = async () => {
    // Reset State
    skip.value = 0;
    entities.value = [];
    hasMoreEntities.value = true;

    await fetchEntities();
  }

  const editEntity = (id) => {
    switch (selectedEntity.value) {
      case 'Tags':
        router.push({ name: 'Admin Edit Tag', params: { id: id } });
        break;
      case 'Videos':
        router.push({ name: 'Admin Edit Video', params: { id: id } });
        break;
      case 'Comments':
        router.push({ name: 'Admin Edit Comment', params: { id: id } });
        break;
      case 'Users':
        router.push({ name: 'Admin Edit User', params: { id: id } });
        break;
      case 'Subscriptions':
        router.push({ name: 'Admin Edit Subscription', params: { id: id } });
        break;
      default:
        return null;
    }
  }

  const deleteEntity = async (id) => {
    try {
      let response = '';

      switch (selectedEntity.value) {
        case 'Tags':
          response = await axios.delete('/api/Admin/delete-entity', {
            params: { entityType: 'Tag', id: id }
          });
          break;
        case 'Videos':
          response = await axios.delete('/api/Admin/delete-entity', {
            params: { entityType: 'Video', id: id }
          });
          break;
        case 'Comments':
          response = await axios.delete('/api/Admin/delete-entity', {
            params: { entityType: 'Comment', id: id }
          });
          break;
        case 'Users':
          response = await axios.delete('/api/Admin/delete-entity', {
            params: { entityType: 'User', id: id }
          });
          break;
        case 'Subscriptions':
          response = await axios.delete('/api/Admin/delete-entity', {
            params: { entityType: 'Subscription', id: id }
          });
          break;
        default:
          return null;
      }

      toast.success(response.data.message);

      reloadEntities();
    } catch (error) {
      toast.error(`Error deleting ${selectedEntity.value}:`, error);
    }
  }
</script>
