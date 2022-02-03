module.exports = {
  apps : [{
    name: "psn-hack-bot",
    script: 'dist/index.js',

    // use when want to TEST BUILT version
    env: {
      
    },

    // use when want to DEPLOY BUILD version
    env_production: {

    }
  }]
};
