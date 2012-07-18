<?php
class Filesystem {

	private function decodeString($string) {
		$string = html_entity_decode($string);
		$string = str_replace('&nsbp;', ' ', $string);
		$string = str_replace('&point;', '.', $string);
		return $string;
	}

	public function dirUp($path, $repeat=1) {
		$parts = explode(DIRECTORY_SEPARATOR, $path);
		for($i = 0; $i < $repeat; $i++) {
			array_pop($parts);
		}
		return implode(DIRECTORY_SEPARATOR, $parts);
	}

	private function sortArray() {
		$args = func_get_args();
		$data = array_shift($args);
		foreach ($args as $n => $field) {
			if($field == 'asc') {
				$args[$n] = SORT_ASC;
				$field = $args[$n];
			}
			if($field == 'desc') {
				$args[$n] = SORT_DESC;
				$field = $args[$n];
			}
			if(is_string($field)) {
				$tmp = array();
				foreach ($data as $key => $row) $tmp[$key] = strtolower($row[$field]);
				$args[$n] = $tmp;
			}
		}
		$args[] = &$data;
		call_user_func_array('array_multisort', $args);
		return array_pop($args);
	}

	public function read($absolutePath, $relativePath, $sortName='filename', $sortOrder=SORT_ASC) {
		for($list = array(), $handle = opendir($absolutePath); (FALSE !== ($file = readdir($handle)));) {
			if(($file != '.' && $file != '..' && $file != 'webint') && (file_exists($path = $absolutePath.DIRECTORY_SEPARATOR.$file))) {
				$entry = array('filename' => $file, 'permission' => substr(sprintf('%o', fileperms($path)), -4));
				$entry['modtime'] = filemtime($path);
				do if(!is_dir($path)) {
					$entry['size'] = filesize($path);
					$entry['mode'] = 'file';
					$entry['dirpath'] = $relativePath.DIRECTORY_SEPARATOR.$file;
					break;
				} else {
					$entry['size'] = '';
					$entry['mode'] = 'folder';
					$entry['dirpath'] = urlencode($relativePath.DIRECTORY_SEPARATOR.$file);
					break;
				} while (FALSE);
				$list[] = $entry;
			}
		}
		closedir($handle);
		$list = $this->sortArray($list, 'mode', 'desc', $sortName, $sortOrder);
		return $list;
	}
	
	public function delete($absolutePath, $files) {
		foreach($files as $file => $on) {
			$file = $absolutePath.'/'.$this->decodeString($file);
			if(!is_writeable($file)) die('You are not allowed to delete this file');
			if(is_dir($file)) rmdir($file);
			if(is_file($file)) unlink($file);
		}
	}
}
?>
