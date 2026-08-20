export const COURSE_CREATE_INITIAL_FORM = {
  name: '',
  descriptionMarkdown: '',
}

export function isCourseCreateFormValid(form) {
  return (form.name?.trim().length ?? 0) >= 3
}

export function buildCourseCreatePayload(form) {
  const payload = { name: form.name.trim() }
  if (form.descriptionMarkdown?.trim()) {
    payload.descriptionMarkdown = form.descriptionMarkdown
  }
  return payload
}
