const backofficePath = './src/Clip/Backoffice';

// two directories up to the test sites
// but build into /src

export const paths = {
    js: [`${backofficePath}/**/*.ts`],
    viewsDev: [`${backofficePath}/**/*.html`],
    viewsProd: [`${backofficePath}/**/*.html`, `!${backofficePath}/**/components/**/*.html`],
    scss: `${backofficePath}/**/*.scss`,
    lang: `./src/Clip/Lang/*.xml`,
    manifest: './src/Clip/package.manifest',
    dest: './App_Plugins/Clip/',
    site: '../../Clip.Umbraco.Site/App_Plugins/Clip/',
};

export const config = {
    hash: new Date().toISOString().split('').reduce((a, b) => (((a << 5) - a) + b.charCodeAt(0)) | 0, 0).toString().substring(1)
};
