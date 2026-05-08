import api from './api';

// category values matching MediaCategory enum:
export const MediaCategory = {
  TrainerProfile: 1,
  TrainerLogo: 2,
  PublicBanner: 3,
  ProgressPhoto: 4,
  PostImage: 5,
  ExerciseImage: 6,
  CheckInPhoto: 7,
  ExerciseVideo: 8,
  PostVideo: 9,
  TransformationPhoto: 10,
};

export const uploadService = {
  upload: async (file, category, options = {}) => {
    const form = new FormData();
    form.append('file', file);
    form.append('category', category);
    if (options.studentId) form.append('studentId', options.studentId);
    if (options.isPublic !== undefined) form.append('isPublic', options.isPublic);
    return api.post('/media/upload', form, { headers: { 'Content-Type': 'multipart/form-data' } });
  },
  delete: (id) => api.delete(`/media/${id}`),
};

