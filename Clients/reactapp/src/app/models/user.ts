export interface User {
  username: string;
  displayName: string;
  token: string;
  image?: string;
}

export interface UserFormValues {
  email: string;
  password: string;
  displayName?: string;
  username?: string;
}

export interface LoginDTO {
  email: string,
  password: string,
 // twoFactorCode: string,
 // twoFactorRecoveryCode: string
}

export interface RegisterDTO {
  email: string,
  password: string
}

export interface RefreshTokenDTO {
  refreshToken: string
}

export interface TokenDTO  {
  tokenType: string,
  accessToken: string,
  expiresIn: number ,
  refreshToken: string
}



