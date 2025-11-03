export const environment = {
  production: true,
  apiUrl: 'https://localhost:7202',
  signalRHubUrl: 'https://localhost:7202/hubs/laf',
  auth: {
    clientId: 'eff19be4-65a4-436b-a03a-df2de6a8715c',
    authority: 'https://login.microsoftonline.com/5181ddb1-868a-41c9-87a7-be21e1d4eb64',
    redirectUri: 'http://localhost:4200',
    postLogoutRedirectUri: 'http://localhost:4200',
    scopes: ['user.read', 'openid', 'profile'],
  },
};
