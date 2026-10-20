import { useState } from 'react'
import { Dropdown } from '@/components/ui/admin/Dropdown'
import { FieldLabel, TextInput } from '@/components/ui/admin/Field'
import { AI_COPY, AI_STATUS, AI_STATUS_LABELS } from '@/constants/ai'
import { emptyToUndefined } from '../aiOutput'
import { AiFormShell } from './AiFormShell'

const STATUS_OPTIONS = Object.values(AI_STATUS).map((value) => ({
  value,
  label: AI_STATUS_LABELS[value],
}))

function toInitialValues(model) {
  return {
    providerId: model?.providerId ?? '',
    modelName: model?.modelName ?? '',
    displayName: model?.displayName ?? '',
    status: model?.status ?? AI_STATUS.active,
  }
}

// Provider chỉ chọn lúc tạo; API cập nhật không cho đổi provider của model.
export function AiModelForm({ model, providers, saving, onSubmit, onCancel }) {
  const editing = Boolean(model)
  const [values, setValues] = useState(() => toInitialValues(model))
  const set = (field) => (value) => setValues((current) => ({ ...current, [field]: value }))
  const providerOptions = providers.map((provider) => ({
    value: provider.id,
    label: provider.name,
  }))

  const submit = async () => {
    const common = {
      modelName: values.modelName.trim(),
      displayName: emptyToUndefined(values.displayName),
      status: values.status,
    }
    const saved = await onSubmit(editing ? common : { ...common, providerId: values.providerId || undefined })
    if (saved && !editing) {
      setValues((current) => ({ ...toInitialValues(null), providerId: current.providerId }))
    }
  }

  return (
    <AiFormShell
      title={editing ? `Sửa model: ${model.displayName}` : AI_COPY.createModel}
      editing={editing}
      saving={saving}
      submitLabel={editing ? AI_COPY.saveChanges : AI_COPY.createModel}
      onSubmit={submit}
      onCancel={onCancel}
    >
      {editing ? (
        <div>
          <FieldLabel htmlFor="ai-model-provider-fixed">{AI_COPY.provider}</FieldLabel>
          <p id="ai-model-provider-fixed" className="mt-1 text-[14px]">{model.providerName}</p>
        </div>
      ) : (
        <Dropdown
          id="ai-model-provider"
          label={AI_COPY.provider}
          value={values.providerId}
          options={providerOptions}
          disabled={saving}
          placeholder="Chọn provider"
          onChange={set('providerId')}
        />
      )}
      <FieldLabel htmlFor="ai-model-name" hint={AI_COPY.modelNameHint}>
        {AI_COPY.modelName}
      </FieldLabel>
      <TextInput
        id="ai-model-name"
        name="modelName"
        value={values.modelName}
        disabled={saving}
        placeholder="gpt-4o"
        onChange={(event) => set('modelName')(event.target.value)}
      />
      <FieldLabel htmlFor="ai-model-display-name" hint={AI_COPY.displayNameHint}>
        {AI_COPY.displayName}
      </FieldLabel>
      <TextInput
        id="ai-model-display-name"
        name="displayName"
        value={values.displayName}
        disabled={saving}
        placeholder="GPT-4o"
        onChange={(event) => set('displayName')(event.target.value)}
      />
      <Dropdown
        id="ai-model-status"
        label={AI_COPY.status}
        value={values.status}
        options={STATUS_OPTIONS}
        disabled={saving}
        onChange={set('status')}
      />
    </AiFormShell>
  )
}
