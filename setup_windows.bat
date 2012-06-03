CD /D "%~dp0"

call git submodule update --init

mklink "%USERPROFILE%\.vimrc" "%~dp0.vimrc"
mklink "%USERPROFILE%\.gvimrc" "%~dp0.gvimrc"
mklink /D "%USERPROFILE%\.vim" "%~dp0.vim"
mklink "%USERPROFILE%\_nya" "%~dp0_nya"
mklink "%USERPROFILE%\_ckw" "%~dp0_ckw"

REM cd .vim/bundle/vimproc/
REM make -f make_msvc32.mak

CD /D "%~dp0"

PAUSE