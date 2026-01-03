{{- define "fcg-usuarios-api.name" -}}
fcg-usuarios-api
{{- end -}}

{{- define "fcg-usuarios-api.fullname" -}}
{{ .Release.Name }}-{{ include "fcg-usuarios-api.name" . }}
{{- end -}}
