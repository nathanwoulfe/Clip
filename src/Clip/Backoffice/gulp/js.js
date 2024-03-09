import gulp from 'gulp';
import gutil from 'gulp-util';
import globby from 'globby';
import through from 'through2';
import log from 'gulplog';
import sourcemaps from 'gulp-sourcemaps';
import babelify from 'babelify';
import browserify from 'browserify';
import source from 'vinyl-source-stream';
import buffer from 'vinyl-buffer';
import embedTemplates from 'gulp-angular-embed-templates';
import tsify from 'tsify';

import { paths, config } from './config';

export function js() {
  return _js(paths.js, `clip.min.js`);
}

function _js(glob, filename) {
  const jsDest = '/backoffice/js/';

  // gulp expects tasks to return a stream, so we create one here.
  var bundledStream = through();
  bundledStream
    // turns the output bundle stream into a stream containing
    // the normal attributes gulp plugins expect.
    .pipe(source(filename))

    // the rest of the gulp task, as you would normally write it.
    // here we're copying from the Browserify + Uglify2 recipe.
    .pipe(buffer())
    .pipe(!config.prod ? sourcemaps.init({ loadMaps: true }) : gutil.noop())
    // Add gulp plugins to the pipeline here.
    .pipe(embedTemplates({
      basePath: './'
    }))
    .on('error', log.error)
    .pipe(!config.prod ? sourcemaps.write() : gutil.noop())
    .pipe(gulp.dest(paths.dest + jsDest));

  // "globby" replaces the normal "gulp.src" as Browserify
  // creates it's own readable stream.
  globby(glob).then(entries => {
    browserify({ entries, debug: !config.prod, extensions: ['.ts', '.js'] })
      .plugin(tsify)
      .transform(babelify.configure({
        presets: ['@babel/preset-env']
      }))
      .bundle()
      .pipe(bundledStream);
  }).catch(err => bundledStream.emit('error', err));

  // finally, we return the stream, so gulp knows when this task is done.
  return bundledStream;
}
