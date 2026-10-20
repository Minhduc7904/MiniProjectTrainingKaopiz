import { useState } from 'react'
import { Dropdown } from '@/components/ui/admin/Dropdown'
import { FieldLabel, TextInput } from '@/components/ui/admin/Field'
import {
  AI_COPY,
  AI_PROVIDER_TYPE,
  AI_PROVIDER_TYPE_LABELS,
  AI_STATUS,
  AI_STATUS_LABELS,
} from '@/constants/ai'
import { emptyToUndefined } from '../aiOutput'
import { AiFormShell } from './AiFormShell'

const TYPE_OPTIONS = Object.values(AI_PROVIDER_TYPE).map((value) => ({
  value,
  label: AI_PROVIDER_TYPE_LABELS[value],
}))
const STATUS_OPTIONS = Object.values(AI_STATUS).map((value) => ({
  value,
  label: AI_STATUS_LABELS[value],
}))

function toInitialValues(provider) {
  return {
    name: provider?.name ?? '',
    providerType: provider?.providerType ?? AI_PROVIDER_TYPE.openai,
    baseUrl: provider?.baseUrl ?? '',
    apiKey: '',
    status: provider?.status ?? AI_STATUS.active,
  }
}

// Gắn key={provider?.id ?? 'new'} ở trang cha để form reset khi đổi bản ghi đang sửa.
export function AiProviderForm({ provider, options, saving, onSubmit, onCancel }) {
  const editing = Boolean(provider)
  const [values, setValues] = useState(() => toInitialValues(provider))
  const set = (field) => (value) => setValues((current) => ({ ...current, [field]: value }))
  const defaultBaseUrl = options?.providerTypes?.find((item) => item.providerType === values.providerType)?.defaultBaseUrl

  const submit = async () => {
    const saved = await onSubmit({
      name: values.name.trim(),
      providerType: values.providerType,
      baseUrl: emptyToUndefined(values.baseUrl),
      apiKey: emptyToUndefined(values.apiKey),
      status: values.status,
    })
    if (saved && !editing) {
      setValues(toInitialValues(null))
    }
  }

  return (
    <AiFormShell
      title={editing ? `Sửa provider: ${provider.name}` : AI_COPY.createProvider}
      editing={editing}
      saving={saving}
      submitLabel={editing ? AI_COPY.saveChanges : AI_COPY.createProvider}
      onSubmit={submit}
      onCancel={onCancel}
    >
      <FieldLabel htmlFor="ai-provider-name">{AI_COPY.name}</FieldLabel>
      <TextInput
        id="ai-provider-name"
        name="name"
        value={values.name}
        disabled={saving}
        placeholder="Ví dụ: OpenAI production"
        onChange={(event) => set('name')(event.target.value)}
      />
      <Dropdown
        id="ai-provider-type"
        label={AI_COPY.providerType}
        value={values.providerType}
        options={TYPE_OPTIONS}
        disabled={saving}
        onChange={set('providerType')}
      />
      <FieldLabel htmlFor="ai-provider-base-url" hint={AI_COPY.baseUrlHint}>
        {AI_COPY.baseUrl}
      </FieldLabel>
      <TextInput
        id="ai-provider-base-url"
        name="baseUrl"
        value={values.baseUrl}
        disabled={saving}
        placeholder={defaultBaseUrl ?? 'https://api.example.com/v1'}
        onChange={(event) => set('baseUrl')(event.target.value)}
      />
      <FieldLabel
        htmlFor="ai-provider-api-key"
        hint={editing ? `${AI_COPY.apiKeyEditHint} Đang lưu: ${provider.apiKeyHint}` : AI_COPY.apiKeyCreateHint}
      >
        {AI_COPY.apiKey}
      </FieldLabel>
      <TextInput
        id="ai-provider-api-key"
        name="apiKey"
        type="password"
        value={values.apiKey}
        disabled={saving}
        placeholder={editing ? '••••••••' : 'sk-...'}
        onChange={(event) => set('apiKey')(event.target.value)}
      />
      <Dropdown
        id="ai-provider-status"
        label={AI_COPY.status}
        value={values.status}
        options={STATUS_OPTIONS}
        disabled={saving}
        onChange={set('status')}
      />
    </AiFormShell>
  )
}
