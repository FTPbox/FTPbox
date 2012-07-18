<script type="text/javascript">
	$(document).ready(function() {
		function reload() {
			$.ajax({
				type: 'GET',
				url: 'ajax.php',
				data: {
					'ajax' : 'reload',
					'relativePath' : '<?php echo $this->relativePath; ?>',
					'absolutePath' : '<?php echo $this->absolutePath; ?>',
					'sortName' : '<?php echo $this->sortName; ?>',
					'sortOrder' : '<?php echo $this->sortOrder; ?>'
				},
				success: function(data){
					$('#browser').html(data);
					period = period * 2;
					clearInterval(periodical);
					periodical = setInterval(reload, period);
					$('.row').find('input').change(function() {
						event.stopPropagation();
						$(this).trigger('click');
					});
					$('.row').click(function() {
						clearInterval(periodical);
						if($(this).find('input').attr('checked')) {
							$(this).removeClass('checked');
							$(this).find('input').attr('checked', false);
						} else {
							$(this).addClass('checked');
							$(this).find('input').attr('checked', true);
						}
					});
				}
			});
		}
		$('#selectall').click(function() {
			clearInterval(periodical);
			if($(this).attr('checked')) {
				$('.row').each(function() {
					$(this).addClass('checked');
					$(this).find('input').attr('checked', true);
				});
			} else {
				$('.row').each(function() {
					$(this).removeClass('checked');
					$(this).find('input').attr('checked', false);
				});
			}
		});
		var period = 100;
		var periodical = setInterval(reload, period);
		var uploader = new qq.FileUploader({
			element: document.getElementById('uploader'),
			action: 'upload.php',
			params: {
				path: '<?php echo $this->absolutePath; ?>'
			}
		});
		$('#toolbox_buttons_upload').fancybox();
	});
</script>
<form id="form_browser" action="index.php" method="POST" enctype="multipart/form-data">
	<div id="uploader_container" style="display:none;">
	<div id="uploader">
	</div>
	</div>
	<div id="toolbox">
		<?php echo $this->up; ?>		
		<div id="toolbox_path">
			<input id="toolbox_path_text" name="p" type="text" value="<?php echo $this->relativePath; ?>" />
			<input id="toolbox_path_submit" type="submit" />
		</div>
		<div id="toolbox_buttons">
			<a id="toolbox_buttons_upload" class="button" href="#uploader" />Upload</a>
			<label id="toolbox_buttons_delete" for="toolbox_buttons_delete_submit" class="button">Delete<input id="toolbox_buttons_delete_submit" type="submit" value="Delete" name="submit_delete" /></label>
		</div>
	</div>
	<div id="browser_head">
		<div id="browser_head_checkbox"><input type="checkbox" id="selectall" /></div>
		<div id="browser_head_link"><a href="?p=<?php echo $this->relativePath; ?>&sortName=filename<?php if($this->sortOrder == 'asc' && $this->sortName == 'filename'): ?>&sortOrder=desc<?php endif; ?>">Name</a></div>
		<div id="browser_head_permission"><a href="?p=<?php echo $this->relativePath; ?>&sortName=permission<?php if($this->sortOrder == 'asc' && $this->sortName == 'permission'): ?>&sortOrder=desc<?php endif; ?>">Permission</a></div>
		<div id="browser_head_size"><a href="?p=<?php echo $this->relativePath; ?>&sortName=size<?php if($this->sortOrder == 'asc' && $this->sortName == 'size'): ?>&sortOrder=desc<?php endif; ?>">Size</a></div>
		<div id="browser_head_modtime"><a href="?p=<?php echo $this->relativePath; ?>&sortName=modtime<?php if($this->sortOrder == 'asc' && $this->sortName == 'modtime'): ?>&sortOrder=desc<?php endif; ?>">Last change</a></div>
	</div>
	<div id="browser">
		<?php echo $this->content; ?>
	</div>
</form>
