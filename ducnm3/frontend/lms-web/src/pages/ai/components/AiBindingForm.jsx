import { useState } from 'react'
import { Dropdown } from '@/components/ui/admin/Dropdown'
import { AI_COPY, AI_PAGES, AI_TASK_LABELS } from '@/constants/ai'
import { adminUi } from '@/theme/admin'
import { AiFormShell } from './AiFormShell'

// Sửa binding = đổi model của một service, nên service bị khóa khi đang sửa.
export function AiBindingForm({ binding, services, models, saving, onSubmit, onCancel }) {
  const editing = Boolean(binding)
  const [serviceName, setServiceName] = useState(binding?.serviceName ?? '')
  const [modelId, setModelId] = useState(binding?.modelId ?? '')
  const serviceOptions = services.map((name) => ({ value: name, label: AI_TASK_LABELS[name] ? `${AI_TASK_LABELS[name]} (${name})` : name }))
  const modelOptions = models.map((model) => ({
    value: model.id,
    label: `${model.providerName} · ${model.displayName}`,
  }))

  const submit = async () => {
    const saved = await onSubmit({
      serviceName,
      payload: { modelId: modelId || undefined },
    })
    if (saved && !editing) {
      setServiceName('')
      setModelId('')
    }
  }

  return (
    <AiFormShell
      title={editing ? `Đổi model cho ${binding.serviceName}` : AI_COPY.assign}
      editing={editing}
      saving={saving}
      submitLabel={editing ? AI_COPY.saveChanges : AI_COPY.assign}
      onSubmit={submit}
      onCancel={onCancel}
    >
      <Dropdown
        id="ai-binding-service"
        label={AI_COPY.service}
        value={serviceName}
        options={serviceOptions}
        disabled={saving || editing}
        placeholder="Chọn service"
        onChange={setServiceName}
      />
      <Dropdown
        id="ai-binding-model"
        label={AI_COPY.model}
        value={modelId}
        options={modelOptions}
        disabled={saving}
        placeholder="Chọn model"
        onChange={setModelId}
      />
      {models.length === 0 ? (
        <p className={`text-[12px] ${adminUi.caption}`}>{AI_PAGES.bindings.noModels}</p>
      ) : null}
    </AiFormShell>
  )
}
