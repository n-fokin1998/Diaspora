export interface AuthApiResponse {
  accessToken: string
  expiresAtUtc: string
  user: {
    id: string
    email: string
    firstName: string
    lastName: string
  }
}

export interface FieldErrors {
  [field: string]: string[]
}

export interface ApiValidationProblem {
  errors: FieldErrors
}
